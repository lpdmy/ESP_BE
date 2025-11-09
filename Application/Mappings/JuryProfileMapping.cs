using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application
{
    public class JuryProfileMapping : Profile
    {
        public JuryProfileMapping()
        {
            CreateMap<JuryActivity, JuryActivityResponseDto>().ForMember(dest => dest.FirstName, opt => opt.MapFrom(p => p.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(p => p.User.LastName));
           
        }
    }
}
