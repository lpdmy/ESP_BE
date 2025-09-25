

using System.Net.NetworkInformation;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.AIService;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;

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
        public PostService(IPostRepository repo, IMapper mapper, IUserRepository userRepo, IHashTagRepository hashTagRepo, Moderation moderation, IAttachmentRepository attachmentRepository, IPostHashTagRepository hashTagRepository) {
            _repo = repo;
            _mapper = mapper;
            _userRepo = userRepo;
            _hashTagRepo = hashTagRepo;
            _moderation = moderation;
            _attachmentRepository = attachmentRepository;
             _hashTagRepository = hashTagRepository;
        }
        public async Task<IEnumerable<PostResponseDto>> getAllPostsGeneral()
        {
            var posts = await _repo.getAllPostIncluding();
            var query = posts.Where(p=>p.ClassGroupId == null && p.ClubId== null && p.IsDeleted ==false).ToList();
            var postDto = _mapper.Map<IEnumerable<PostResponseDto>>(query);
            return postDto;
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
            };
            foreach (var tag in (dto.Hashtags ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var hashtag = _hashTagRepo.GetByNameAsync(tag).Result;

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
            foreach (var attachment in (dto.AttachmentUrls ?? new List<AttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachment.Url))
                {
                    post.Attachments.Add(new Attachment
                    {
                        FileUrl = attachment.Url,
                        FileType = attachment.FileType,
                        Post = post
                    });
                }
            }

            var moderationBody = await _moderation.Moderate(new ModerationRequest
            {
                Input = dto.Body,
            });
            var moderationTitle = await _moderation.Moderate(new ModerationRequest
            {
                Input = dto.Title,
            });
            if (moderationBody.IsFlagged == true || moderationTitle.IsFlagged)
            { 
               throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            }    

            await _repo.AddAsync(post);
            var postDto = _mapper.Map<PostResponseDto>(post);
            return postDto;
        }
        public async Task<IEnumerable<PostResponseDto>> GetPostByUserId(User user, string sortOrder = "newest")
        {
            var posts = await _repo.getAllPostIncluding();
            var userPost = posts.Where(p => p.UserId == user.Id && p.IsDeleted == false);
            var order = sortOrder?.ToLower() ?? "newest";
            userPost = order switch
            {
                "oldest" => userPost.OrderBy(p => p.CreatedAt),
                "newest" => userPost.OrderByDescending(p => p.CreatedAt),
                _ => userPost.OrderByDescending(p => p.CreatedAt)
            };

            return _mapper.Map<IEnumerable<PostResponseDto>>(userPost.ToList());
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

            
            if (!string.IsNullOrWhiteSpace(dto.Body))
            {
                var moderationBody = await _moderation.Moderate(new ModerationRequest { Input = dto.Body });
                if (moderationBody.IsFlagged)
                    throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            }

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var moderationTitle = await _moderation.Moderate(new ModerationRequest { Input = dto.Title });
                if (moderationTitle.IsFlagged)
                    throw new BadRequestException(ErrorMessages.Post.PostIsFlaged);
            }

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

            if (dto.MentionUsernames != null)
            {
                post.PostMentions.Clear();
                foreach (var id in dto.MentionUsernames.Distinct())
                {
                    var mentionedUser = await _userRepo.GetByIdAsync(id);
                    if (mentionedUser != null)
                    {
                        post.PostMentions.Add(new PostMention
                        {
                            MentionedUserId = mentionedUser.Id,
                            Post = post
                        });
                    }
                }
            }

            
                await _attachmentRepository.DeleteAttachmentByPostId(post.Id);

                foreach (var attachment in dto.AttachmentUrls)
                {
                    if (!string.IsNullOrWhiteSpace(attachment.Url))
                    {
                        post.Attachments.Add(new Attachment
                        {
                            FileUrl = attachment.Url,
                            FileType = attachment.FileType,
                            Post = post
                        });
                    }
                }
            await _repo.UpdateAsync(post);
            return _mapper.Map<PostResponseDto>(post);
        }


    }
}
