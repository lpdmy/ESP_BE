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
        opt => opt.MapFrom(src => src.Comments.Select(c => c.Content)))

    .ForMember(dest => dest.LikeCount,
        opt => opt.MapFrom(src => src.PostLikes.Count))

    .ForMember(dest => dest.ReportCount,
        opt => opt.MapFrom(src => src.PostReports.Count));
        }
    }
}
