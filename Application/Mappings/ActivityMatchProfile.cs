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
                .ForMember(dest => dest.ClassGroup1Name, opt => opt.MapFrom(src => 
                    src.ClassGroup1 != null 
                        ? (src.ClassGroup1.Grade.HasValue && !string.IsNullOrEmpty(src.ClassGroup1.Name)
                            ? $"{src.ClassGroup1.Grade.Value}{src.ClassGroup1.Name}"
                            : src.ClassGroup1.Name ?? null)
                        : null))
                .ForMember(dest => dest.ClassGroup2Name, opt => opt.MapFrom(src => 
                    src.ClassGroup2 != null 
                        ? (src.ClassGroup2.Grade.HasValue && !string.IsNullOrEmpty(src.ClassGroup2.Name)
                            ? $"{src.ClassGroup2.Grade.Value}{src.ClassGroup2.Name}"
                            : src.ClassGroup2.Name ?? null)
                        : null))
                .ForMember(dest => dest.WinnerClassGroupName, opt => opt.MapFrom(src => 
                    src.WinnerClassGroup != null 
                        ? (src.WinnerClassGroup.Grade.HasValue && !string.IsNullOrEmpty(src.WinnerClassGroup.Name)
                            ? $"{src.WinnerClassGroup.Grade.Value}{src.WinnerClassGroup.Name}"
                            : src.WinnerClassGroup.Name ?? null)
                        : null));

            // CreateMatchDto to ActivityMatch
            CreateMap<CreateMatchDto, ActivityMatch>();

            // UpdateMatchResultDto will be mapped manually in service
        }
    }
}

