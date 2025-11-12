using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            // Activity to DTOs
            CreateMap<Activity, ActivityResponseDto>()
                .ForMember(dest => dest.Rules, opt => opt.MapFrom(src => src.Rules.Where(r => !r.IsDeleted).Select(r => r.RuleText)))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.ActivityParticipants.Where(p => !p.IsDeleted)))
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Where(s => !s.IsDeleted)))
                .ForMember(dest => dest.Speakers, opt => opt.MapFrom(src => src.Speakers.Where(s => !s.IsDeleted).OrderBy(s => s.Order)))
                .ForMember(dest => dest.Programs, opt => opt.MapFrom(src => src.Programs.Where(p => !p.IsDeleted).OrderBy(p => p.Order)))
                .ForMember(dest => dest.Awards, opt => opt.MapFrom(src => src.ActivityRewards.Where(r => !r.IsDeleted)))
                .ForMember(dest => dest.NumberOfParticipants, opt => opt.MapFrom(src => src.ActivityParticipants.Count(p => !p.IsDeleted)));

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
        }
    }
}
