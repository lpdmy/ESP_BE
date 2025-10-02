using System.Text.Json;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ClubCreationRequestProfile : Profile
    {
        public ClubCreationRequestProfile()
        {
            CreateMap<ClubCreationRequest, ClubCreationResponseDto>()
                .ForMember(dest => dest.RequestedByUserId,
                    opt => opt.MapFrom(src => src.RequestedByUser.Id))
                .ForMember(dest => dest.RequestedByName,
                    opt => opt.MapFrom(src => src.RequestedByUser.LastName + " " + src.RequestedByUser.FirstName))
                .ForMember(dest => dest.RequestedByEmail,
                    opt => opt.MapFrom(src => src.RequestedByUser.Email));
        }
    }
}
