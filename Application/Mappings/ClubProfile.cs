using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ClubProfile : Profile
    {
        public ClubProfile()
        {
            CreateMap<Club, ClubResponseDto>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src =>
                    src.Mentor != null ? src.Mentor.LastName + " " + src.Mentor.FirstName : null))
                .ForMember(dest => dest.PresidentName, opt => opt.MapFrom(src =>
                    src.President != null ? src.President.LastName + " " + src.President.FirstName : null))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src =>
                    src.ClubMembers
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.Role == "President")
                     ))
                ;
            CreateMap<ClubMember, ClubMemberDto>()
    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
        $"{src.User.LastName} {src.User.FirstName}".Trim()))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
    .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
    .ForMember(dest => dest.UserRole,
    opt => opt.MapFrom(src => (int?)src.User.Role))
    ;
            CreateMap<ClubCreationRequest, ClubCreationResponseDto>()
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src =>
                    src.RequestedByUser != null ? src.RequestedByUser.LastName + " " + src.RequestedByUser.FirstName : null))
                .ForMember(dest => dest.RequestedByEmail, opt => opt.MapFrom(src =>
                    src.RequestedByUser != null ? src.RequestedByUser.Email : null));
                
        }
    }
}
