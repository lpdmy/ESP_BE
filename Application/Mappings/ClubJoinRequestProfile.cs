using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application
{
    public class ClubJoinRequestProfile : Profile
    {
        public ClubJoinRequestProfile()
        {
            CreateMap<ClubJoinRequest, ClubJoinRequestDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
        $"{src.User.LastName} {src.User.FirstName}".Trim()))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.AvatarUrl));
            CreateMap<ClubJoinRequestDto, ClubJoinRequest>();
        }
    }
}
