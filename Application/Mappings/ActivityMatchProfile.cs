using AutoMapper;
using EduShpere.Application.DTOs.ActivityMatchDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ActivityMatchProfile : Profile
    {
        public ActivityMatchProfile()
        {
            // ActivityMatch to MatchResponseDto
            CreateMap<ActivityMatch, MatchResponseDto>()
                .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport != null ? src.Sport.SportName : null))
                .ForMember(dest => dest.ClassGroup1Name, opt => opt.MapFrom(src => src.ClassGroup1 != null ? src.ClassGroup1.Name : null))
                .ForMember(dest => dest.ClassGroup2Name, opt => opt.MapFrom(src => src.ClassGroup2 != null ? src.ClassGroup2.Name : null))
                .ForMember(dest => dest.WinnerClassGroupName, opt => opt.MapFrom(src => src.WinnerClassGroup != null ? src.WinnerClassGroup.Name : null));

            // CreateMatchDto to ActivityMatch
            CreateMap<CreateMatchDto, ActivityMatch>();

            // UpdateMatchResultDto will be mapped manually in service
        }
    }
}

