using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;
using System.Text.Json;

namespace EduShpere.Application.Mappings
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            // Activity to DTOs
            CreateMap<Activity, ActivityResponseDto>()
                .ForMember(dest => dest.Rules, opt => opt.MapFrom(src => src.Rules != null ? src.Rules.Where(r => !r.IsDeleted).Select(r => r.RuleText).ToList() : new List<string>()))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.ActivityParticipants != null ? src.ActivityParticipants.Where(p => !p.IsDeleted).ToList() : new List<ActivityParticipant>()))
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports != null ? src.Sports.Where(s => !s.IsDeleted).ToList() : new List<ActivitySport>()))
                .ForMember(dest => dest.Speakers, opt => opt.MapFrom(src => src.Speakers != null ? src.Speakers.Where(s => !s.IsDeleted).OrderBy(s => s.Order).ToList() : new List<ActivitySpeaker>()))
                .ForMember(dest => dest.Programs, opt => opt.MapFrom(src => src.Programs != null ? src.Programs.Where(p => !p.IsDeleted).OrderBy(p => p.Order).ToList() : new List<ActivityProgram>()))
                .ForMember(dest => dest.Awards, opt => opt.MapFrom(src => src.ActivityRewards != null ? src.ActivityRewards.Where(r => !r.IsDeleted).ToList() : new List<ActivityReward>()))
                .ForMember(dest => dest.NumberOfParticipants, opt => opt.MapFrom(src => src.ActivityParticipants != null ? src.ActivityParticipants.Count(p => !p.IsDeleted) : 0))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.OnlyTeacherCanRegister, opt => opt.MapFrom(src => src.OnlyTeacherCanRegister ?? false))
                .ForMember(dest => dest.RegistrationReward, opt => opt.MapFrom(src => src.RegistrationReward != null && !src.RegistrationReward.IsDeleted ? src.RegistrationReward : null))
                .ForMember(dest => dest.ActivityDetail, opt => opt.MapFrom(src => src.ActivityDetail != null && !src.ActivityDetail.IsDeleted ? src.ActivityDetail : null))
                .ForMember(dest => dest.GradingSettings, opt => opt.Ignore()) // Ignore default mapping, handle in AfterMap
                .AfterMap((src, dest) => 
                {
                    // Deserialize GradingSettings from JSON string and set Enabled from IsGrade (handle nullable)
                    if (src.IsGrade == true && !string.IsNullOrEmpty(src.GradingSettings))
                    {
                        try
                        {
                            var criteria = JsonSerializer.Deserialize<List<string>>(src.GradingSettings);
                            dest.GradingSettings = new GradingSettingsDto
                            {
                                Criteria = criteria ?? new List<string>()
                            };
                        }
                        catch
                        {
                            dest.GradingSettings = new GradingSettingsDto { Criteria = new List<string>() };
                        }
                    }
                    else
                    {
                        dest.GradingSettings = null;
                    }
                });

            // Related entities to DTOs
            CreateMap<ActivitySpeaker, ActivitySpeakerDto>();
            CreateMap<ActivityProgram, ActivityProgramDto>();
            CreateMap<ActivitySport, ActivitySportDto>();
            CreateMap<ActivityDetail, ActivityDetailDto>();
            CreateMap<ActivityRegistrationReward, ActivityRegistrationRewardDto>();
            CreateMap<ActivityParticipant, ActivityParticipantDto>();
            CreateMap<ActivityReward, ActivityAwardDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Rank))
                .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.StarPoints))
                .ForMember(dest => dest.StarPoints, opt => opt.MapFrom(src => src.StarPoints));

            // DTOs to Entities (for Create/Update)
            CreateMap<ActivitySpeakerDto, ActivitySpeaker>();
            CreateMap<ActivityProgramDto, ActivityProgram>();
            CreateMap<ActivitySportDto, ActivitySport>();
            CreateMap<ActivityDetailDto, ActivityDetail>();
            CreateMap<ActivityRegistrationRewardDto, ActivityRegistrationReward>();
            CreateMap<ActivityAwardDto, ActivityReward>()
                .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Name ?? src.Rank))
                .ForMember(dest => dest.StarPoints, opt => opt.MapFrom(src => src.StarPoints > 0 ? src.StarPoints : src.Points));
            CreateMap<Activity, ActivityResponseDto>().ForMember(dest => dest.Rules,
                       opt => opt.MapFrom(src => src.Rules.Select(r => r.RuleText)))
                       .ForMember(dest => dest.numberOfSubmission, opt => opt.MapFrom(src => src.Submissions.Count()))
                       .ForMember(dest => dest.numberOfPendingSubmission, opt => opt.MapFrom(src => src.Submissions.Where(p => p.Score == null).Count()))
                       .ForMember(dest => dest.numberOfCompletedSubmission, opt => opt.MapFrom(src => src.Submissions.Where(p => p.Score != null).Count()));
            CreateMap<ActivityResponseDto, Activity>();
        }
    }
}
