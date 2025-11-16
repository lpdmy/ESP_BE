using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<Activity, ActivityResponseDto>().ForMember(dest => dest.Rules,
                       opt => opt.MapFrom(src => src.Rules.Select(r => r.RuleText)))
                       .ForMember(dest => dest.numberOfSubmission, opt => opt.MapFrom(src => src.Submissions.Count()))
                       .ForMember(dest => dest.numberOfPendingSubmission, opt => opt.MapFrom(src => src.Submissions.Where(p => p.Score == null).Count()))
                       .ForMember(dest => dest.numberOfCompletedSubmission, opt => opt.MapFrom(src => src.Submissions.Where(p => p.Score != null).Count()));
            CreateMap<ActivityResponseDto, Activity>();
        }
    }
}
