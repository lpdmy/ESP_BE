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
    public class ClubMemberProfile : Profile
    {
        public ClubMemberProfile()
        {
            CreateMap<ClubMember, ClubMemberResponseDto>()
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Club.Name) ? "" : src.Club.Name))
                .ForMember(dest => dest.CategoryName,opt => opt.MapFrom(src => src.Club.Category.Name ?? ""));
        } 
    }
}
