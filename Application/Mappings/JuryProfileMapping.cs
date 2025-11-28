using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application
{
    public class JuryProfileMapping : Profile
    {
        public JuryProfileMapping()
        {
            CreateMap<Activity, ActivityJuryResponseDto>()
           .ForMember(dest => dest.numberOfSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count()))
           .ForMember(dest => dest.numberOfPendingSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count(p => p.Score == null)))
           .ForMember(dest => dest.numberOfCompletedSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count(p => p.Score != null)))
           .ForMember(dest => dest.NumberOfParticipants,
               opt => opt.MapFrom(src => src.ActivityParticipants.Count()));
            CreateMap<JuryActivity, JuryActivityResponseDto>()
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Activity,
                    opt => opt.MapFrom(src => src.Activity))
                .ForMember(dest => dest.Avatar,
                    opt => opt.MapFrom(src => src.User.AvatarUrl));
            CreateMap<JuryAssignment, JuryAssignmentDto>()
                .ForMember(dest => dest.GradeSetting, opt =>opt.MapFrom(src=>src.Submission.Activity.GradingSettings))
                .ForMember(dest => dest.JuryName, opt => opt.MapFrom(src => src.User.LastName + " " + src.User.FirstName));
        }
    }
}
