

using System.Net.NetworkInformation;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.AIService;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduShpere.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepo;
        private readonly IHashTagRepository _hashTagRepo;
        private readonly Moderation _moderation;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IPostHashTagRepository _hashTagRepository;
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IClubMemberRepository _clubMemberRepository;
        public PostService(IPostRepository repo, IMapper mapper, IUserRepository userRepo, IHashTagRepository hashTagRepo, Moderation moderation, IAttachmentRepository attachmentRepository, IPostHashTagRepository hashTagRepository, IPostLikeRepository postLikeRepository, IClubMemberRepository clubMemberRepository) {
            _repo = repo;
            _mapper = mapper;
            _userRepo = userRepo;
            _hashTagRepo = hashTagRepo;
            //_moderation = moderation;
            _attachmentRepository = attachmentRepository;
             _hashTagRepository = hashTagRepository;
            _postLikeRepository = postLikeRepository;
            _clubMemberRepository = clubMemberRepository;
        }
        private async Task<IEnumerable<PostResponseDto>> GetPostsCore(
    Func<IQueryable<Post>, IQueryable<Post>> filter,
    int? currentUserId,
    string sortOrder = "newest")
        {
            var query = filter(_repo.GetAllPostIncluding());

            // Sort
            sortOrder = sortOrder?.ToLower() ?? "newest";
            query = sortOrder switch
            {
                "oldest" => query.OrderBy(p => p.CreatedAt),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "mostliked" => query.OrderByDescending(p => p.PostLikes.Count),
                "mostcommented" => query.OrderByDescending(p => p.Comments.Count),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query.Select(p => new PostResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                UserFullName = p.User.LastName + " " + p.User.FirstName,
                ClassGroupId = p.ClassGroupId,
                ClubId = p.ClubId,
                ClubName = p.Club != null ? p.Club.Name : null,
                PrivacyLevel = p.PrivacyLevel,
                Status = p.Status,
                CallToAction = p.CallToAction,
                IsDeleted = p.IsDeleted,
                Hashtags = p.PostHashtags.Select(ph => ph.Hashtag.Name).ToList(),
                MentionUsernames = p.PostMentions.Select(m => m.MentionedUser.Username).ToList(),
                Comments = p.Comments.Select(c => c.Content).ToList(),
                Attachments = p.Attachments.Select(a => new PostAttachmentDto
                {
                    Url = a.FileUrl ?? string.Empty,
                    FileName = a.FileName ?? string.Empty, // Chỉ trả về khi có giá trị
                    FileType = a.FileType ?? string.Empty
                }).ToList(),
                AttachmentUrls = p.Attachments.Select(a => a.FileUrl).ToList(),
                LikeCount = p.PostLikes.Count,
                ReportCount = p.PostReports.Count,
                CreatedAt = p.CreatedAt ?? DateTime.Now,
                AvatarUrl = p.User.AvatarUrl,
                IsLikedByCurrentUser = p.PostLikes.Any(l => l.UserId == currentUserId) 
            }).ToListAsync();
        }

        public Task<IEnumerable<PostResponseDto>> GetAllPostsGeneral(User currentUser)
        {
            return GetPostsCore(
                posts => posts.Where(p => p.ClassGroupId == null && p.ClubId == null && !p.IsDeleted),
                currentUser.Id
            );
        }


        public async Task<PostResponseDto> CreatePost(CreatePostDto dto , User user)
        {
            var post = new Post
            {
                Title = dto.Title,
                Body = dto.Body,
                UserId = user.Id,
                ClassGroupId = dto.ClassGroupId,
                ClubId = dto.ClubId,
                PrivacyLevel = dto.PrivacyLevel,
                Status = dto.Status,
                CallToAction = dto.CallToAction,
                CreatedAt = DateTime.Now,
                CreatedBy = user.Id,
            };
            foreach (var tag in (dto.Hashtags ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var hashtag = await _hashTagRepo.GetByNameAsync(tag);

                if (hashtag == null)
                {
                    hashtag = new Hashtag { Name = tag };
                    await _hashTagRepo.AddAsync(hashtag);
                }

                post.PostHashtags.Add(new PostHashtag
                {
                    Hashtag = hashtag,
                    Post = post
                });
            }
            foreach (var id in (dto.MentionUsernames ?? new List<int>()).Distinct())
            {
                var userId = await _userRepo.GetByIdAsync(id);
                if (userId != null)
                {
                    post.PostMentions.Add(new PostMention
                    {
                        MentionedUserId = userId.Id,
                        Post = post
                    });
                }
            }
            foreach (var attachment in (dto.AttachmentUrls ?? new List<PostAttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachment.Url))
                {
                    post.Attachments.Add(new Attachment
                    {
                        FileUrl = attachment.Url,
                        FileName = !string.IsNullOrWhiteSpace(attachment.FileName) ? attachment.FileName : null,
                        FileType = attachment.FileType,
                        Post = post
                    });
                }
            }

            //var moderationBody = await _moderation.Moderate(new ModerationRequest
            //{
            //    Input = dto.Body,
            //});
            //var moderationTitle = await _moderation.Moderate(new ModerationRequest
            //{
            //    Input = dto.Title,
            //});
            //if (moderationBody.IsFlagged == true || moderationTitle.IsFlagged)
            //{ 
            //   throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            //}    

            await _repo.AddAsync(post);
            var postDto = _mapper.Map<PostResponseDto>(post);
            return postDto;
        }
        public Task<IEnumerable<PostResponseDto>> GetPostByUserId(User user, string sortOrder = "newest")
        {
            return GetPostsCore(
                posts => posts.Where(p => p.UserId == user.Id && !p.IsDeleted),
                user.Id,
                sortOrder
            );
        }

        public Task<IEnumerable<PostResponseDto>> GetPostsByClassGroup(int classGroupId, User currentUser)
        {
            return GetPostsCore(
                posts => posts.Where(p => p.ClassGroupId == classGroupId && !p.IsDeleted),
                currentUser.Id
            );
        }

        public async Task<PostResponseDto> DeletePost(int id)
        {
           var post = await _repo.GetByIdAsync(id);
            if (post == null)
                throw new BadRequestException(ErrorMessages.Post.PostNotFound);
            await _repo.DeleteSoft(post.Id);
            return null;
        }
        public async Task<PostResponseDto> UpdatePost( UpdatePostDto dto)
        {
            var post = await _repo.GetByIdAsync(dto.Id);

            if (post == null)
            {
                throw new NotFoundException("Post not found.");
            }

            
            //if (!string.IsNullOrWhiteSpace(dto.Body))
            //{
            //    var moderationBody = await _moderation.Moderate(new ModerationRequest { Input = dto.Body });
            //    if (moderationBody.IsFlagged)
            //        throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            //}

            //if (!string.IsNullOrWhiteSpace(dto.Title))
            //{
            //    var moderationTitle = await _moderation.Moderate(new ModerationRequest { Input = dto.Title });
            //    if (moderationTitle.IsFlagged)
            //        throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            //}
            post.UpdatedAt = DateTime.UtcNow;
            post.PostHashtags.Clear();
           await _hashTagRepository.DeleteByPostId(post.Id);
            foreach (var tag in dto.Hashtags.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(tag)) continue;

                    var hashtag = await _hashTagRepo.GetByNameAsync(tag);
                    if (hashtag == null)
                    {
                        hashtag = new Hashtag { Name = tag };
                        await _hashTagRepo.AddAsync(hashtag);
                    }

                    post.PostHashtags.Add(new PostHashtag
                    {
                        Hashtag = hashtag,
                        Post = post
                    });
                 }
                await _attachmentRepository.DeleteAttachmentByPostId(post.Id);

                foreach (var attachment in dto.AttachmentUrls)
                {
                    if (!string.IsNullOrWhiteSpace(attachment.Url))
                    {
                        post.Attachments.Add(new Attachment
                        {
                            FileUrl = attachment.Url,
                            FileName = !string.IsNullOrWhiteSpace(attachment.FileName) ? attachment.FileName : null,
                            FileType = attachment.FileType,
                            Post = post
                        });
                    }
                }
            post.PrivacyLevel = dto.PrivacyLevel;
            post.Body = dto.Body;
            post.Title = dto.Title;

            await _repo.UpdateAsync(post);
            return _mapper.Map<PostResponseDto>(post);
        }
        public async Task<PostResponseDto?> PostLike(CreatePostLikeDto dto, User user)
        {
            var post = await _repo.GetAllPostIncluding()
                .FirstOrDefaultAsync(p => p.Id == dto.PostId);

            if (post == null) return null;

            var existingLike = post.PostLikes.FirstOrDefault(l => l.UserId == user.Id);

            if (existingLike != null)
            {
                await _postLikeRepository.DeleteAsync(existingLike.Id);
            }
            else
            {
                var postLike = new PostLike
                {
                    PostId = dto.PostId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.Id,
                };
                await _postLikeRepository.AddAsync(postLike);
            }

            var updatedPost = await _repo.GetAllPostIncluding()
                .FirstOrDefaultAsync(p => p.Id == dto.PostId);

            return _mapper.Map<PostResponseDto>(
                updatedPost,
                opt => opt.Items["currentUserId"] = user.Id
            );
        }
        public async Task<IEnumerable<PostResponseDto>> GetAllPostsClub(int clubid,User user)
        {
            var post = await GetPostsCore(
                posts => posts.Where(p => p.ClassGroupId == null && p.ClubId == clubid && !p.IsDeleted && p.Status == Domain.Enum.PostStatus.Approved),currentUserId:user.Id
            );
            if (post == null || !post.Any())
            {
                throw new BadRequestException(ErrorMessages.Post.ListNotFound);
            }
            bool isMember = await _clubMemberRepository.IsInClub(user.Id, clubid);
            if (!isMember)
            {
                post = post.Where(p => p.PrivacyLevel == 0);
            }

            return post;
        }
        public async Task<PaginationResponseDto<PostResponseDto>> GetAllPostsClubPending(int clubid,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllPostIncludingByClubId(clubid).Where(p=>p.Status==PostStatus.Pending);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<PostResponseDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<PostResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<IEnumerable<PostResponseDto>> GetAllPostsPending(int clubid)
        {
            var post = await GetPostsCore(
                posts => posts.Where(p => p.ClassGroupId == null && p.ClubId == clubid && !p.IsDeleted && p.Status == Domain.Enum.PostStatus.Pending), currentUserId: null
            );

            if (post == null || !post.Any())
            {
                throw new BadRequestException(ErrorMessages.Post.ListNotFound);
            }

            return post;
        }
        public async Task<PostResponseDto>ApprovePost(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            if (post == null)
                throw new BadRequestException(ErrorMessages.Post.PostNotFound);
            post.Status = PostStatus.Approved;
            await _repo.UpdateAsync(post);
            return _mapper.Map<PostResponseDto>(post);
        }
        public async Task<PostResponseDto> RejectPost(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            if (post == null)
                throw new BadRequestException(ErrorMessages.Post.PostNotFound);
            post.Status = PostStatus.Reject;
            await _repo.UpdateAsync(post);
            return _mapper.Map<PostResponseDto>(post);
        }

    }
}
