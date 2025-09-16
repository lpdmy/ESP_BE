using AutoMapper;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Mappings
{
    public class StudentProfileMapping : Profile
    {
        public StudentProfileMapping()
        {
            CreateMap<CreateUpdateStudentProfileDto, StudentProfile>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<StudentProfile, StudentProfileDto>()
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.User.Birthdate))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));

            CreateMap<StudentProfile, GetStudentProfileDto>()
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.User.Birthdate))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.ClassGroupName, opt => opt.MapFrom(src => 
                    src.User.ClassGroupMembers.FirstOrDefault() != null ? 
                    src.User.ClassGroupMembers.FirstOrDefault().ClassGroup.Name : null))
                .ForMember(dest => dest.ClassGroupId, opt => opt.MapFrom(src => 
                    src.User.ClassGroupMembers.FirstOrDefault() != null ? 
                    src.User.ClassGroupMembers.FirstOrDefault().ClassGroupId : (int?)null));

            CreateMap<UpdateStudentInfoDto, StudentProfile>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
