using AutoMapper;
using EduShpere.Application.DTOs.AttachmentDto;
using EduShpere.Application.DTOs.SystemAnnouncementDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings;

public class SystemAnnouncementProfile : Profile
{
    public SystemAnnouncementProfile()
    {
        // Create mapping
        CreateMap<CreateSystemAnnouncementDto, Post>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.IsSystemAnnouncement, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.PrivacyLevel, opt => opt.MapFrom(src => Domain.Enum.PostVisibility.Public))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enum.PostStatus.Published))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroup, opt => opt.Ignore())
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionItems, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.PostInterests, opt => opt.Ignore())
            .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
            .ForMember(dest => dest.PostReports, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.PostHashtags, opt => opt.Ignore())
            .ForMember(dest => dest.PostMentions, opt => opt.Ignore());

        // Update mapping
        CreateMap<UpdateSystemAnnouncementDto, Post>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.IsSystemAnnouncement, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.PrivacyLevel, opt => opt.MapFrom(src => Domain.Enum.PostVisibility.Public))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsVisible ? Domain.Enum.PostStatus.Published : Domain.Enum.PostStatus.Pending))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroup, opt => opt.Ignore())
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionItems, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.PostInterests, opt => opt.Ignore())
            .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
            .ForMember(dest => dest.PostReports, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.PostHashtags, opt => opt.Ignore())
            .ForMember(dest => dest.PostMentions, opt => opt.Ignore());

        // Response mappings
        CreateMap<Post, SystemAnnouncementDto>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Body))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.Status == Domain.Enum.PostStatus.Published))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<Post, SystemAnnouncementDetailDto>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Body))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.Status == Domain.Enum.PostStatus.Published))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<Post, SystemAnnouncementListItemDto>()
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.Status == Domain.Enum.PostStatus.Published))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments.Count));

        CreateMap<Attachment, SystemAttachmentDto>();
    }
}
