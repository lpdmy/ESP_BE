using System.Diagnostics;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, PostResponseDto>()
    .ForMember(dest => dest.ClubName,
        opt => opt.MapFrom(src => src.Club != null ? src.Club.Name : null))

    .ForMember(dest => dest.UserFullName,
    opt => opt.MapFrom(src =>
        src.User != null
            ? string.Join(" ",
                (src.User.LastName ?? "").Trim(),
                (src.User.FirstName ?? "").Trim()
              ).Trim()
            : string.Empty
    ))
    .ForMember(dest => dest.AttachmentUrls,
        opt => opt.MapFrom(src => src.Attachments.Select(a => a.FileUrl)))

    .ForMember(dest => dest.Hashtags,
        opt => opt.MapFrom(src => src.PostHashtags.Select(ph => ph.Hashtag.Name)))

    .ForMember(dest => dest.MentionUsernames,
        opt => opt.MapFrom(src => src.PostMentions.Select(pm => pm.MentionedUser.Username)))

    .ForMember(dest => dest.Comments,
        opt => opt.MapFrom(src => src.Comments.Where(c => !c.IsDeleted).Select(c => c.Content)))

    .ForMember(dest => dest.LikeCount,
        opt => opt.MapFrom(src => src.PostLikes.Count))
    .ForMember(dest=> dest.AvatarUrl, 
      opt => opt.MapFrom(src => src.User.AvatarUrl))
    .AfterMap((src, dest) =>
    {
        Debug.WriteLine($"User.AvatarUrl = {src.User?.AvatarUrl}, Mapped dest.AvatarUrl = {dest.AvatarUrl}");
    })

    .ForMember(dest => dest.ReportCount,
        opt => opt.MapFrom(src => src.PostReports.Count))
    .ForMember(dest => dest.IsLikedByCurrentUser,
                opt => opt.MapFrom((src, dest, destMember, ctx) =>
                    src.PostLikes.Any(l => l.UserId == (int)ctx.Items["currentUserId"])));
        }
    }
}
