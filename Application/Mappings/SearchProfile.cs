using AutoMapper;
using EduShpere.Application.DTOs.SearchDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class SearchProfile : Profile
    {
        public SearchProfile()
        {
            // User search mapping
            CreateMap<User, UserSearchResultDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName ?? ""))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName ?? ""))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? ""))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.HasValue ? (int)src.Role.Value : 0))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => 
                    src.StudentProfile != null ? src.StudentProfile.StudentNumber : null))
                .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => 
                    src.TeacherProfile != null ? src.TeacherProfile.TeacherCode : null))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => 
                    src.ClassGroupMembers != null && src.ClassGroupMembers.Any() 
                        ? src.ClassGroupMembers.First().ClassGroup.Name 
                        : null))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => 
                    src.TeacherProfile != null ? src.TeacherProfile.Department : null));

            // Post search mapping
            CreateMap<Post, PostSearchResultDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                    $"{src.User.FirstName} {src.User.LastName}".Trim()))
                .ForMember(dest => dest.AuthorAvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CommentsCount, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Hashtags, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Highlight, opt => opt.Ignore()); // Set in service

            // Activity search mapping
            CreateMap<Activity, ActivitySearchResultDto>()
                .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer))
                .ForMember(dest => dest.ParticipantsCount, opt => opt.Ignore()); // Set in service

            // Club search mapping (when Club model is available)
            // CreateMap<Club, ClubSearchResultDto>()
            //     .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => 
            //         $"{src.CreatedByUser.Firstname} {src.CreatedByUser.Lastname}".Trim()))
            //     .ForMember(dest => dest.MembersCount, opt => opt.Ignore()) // Set in service
            //     .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted));

            // Hashtag search mapping (when Hashtag model is available)
            CreateMap<Hashtag, HashtagSearchResultDto>()
                .ForMember(dest => dest.PostsCount, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Trending, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.LastUsed, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Description, opt => opt.Ignore()); // Set in service
        }
    }
}
