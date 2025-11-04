using AutoMapper;
using EduShpere.Domain.Models;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.AuthDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduShpere.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentProfile != null ? src.StudentProfile.StudentNumber : null))
                .ForMember(dest => dest.EnrollmentYear, opt => opt.MapFrom(src => src.StudentProfile != null ? src.StudentProfile.EnrollmentYear : null))
                .ForMember(dest => dest.ClassGroupId, opt => opt.MapFrom(src => src.ClassGroupMembers != null && src.ClassGroupMembers.Any()
                    ? src.ClassGroupMembers.First().ClassGroupId
                    : (int?)null))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassGroupMembers != null && src.ClassGroupMembers.Any() 
                    ? src.ClassGroupMembers.First().ClassGroup.Name 
                    : null))
                .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => src.TeacherProfile != null ? src.TeacherProfile.TeacherCode : null))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.TeacherProfile != null ? src.TeacherProfile.Department : null))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.TeacherProfile != null ? src.TeacherProfile.Position : null))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.UserRights
                    .Where(ur => !ur.IsDeleted)
                    .Select(ur => ur.Right.Code)
                    .ToList()
            ));

            CreateMap<UserDto, User>();
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.UserRights
                    .Where(ur => !ur.IsDeleted)
                    .Select(ur => ur.Right.Code)
                    .ToList()
            ));
        }
    }
}
