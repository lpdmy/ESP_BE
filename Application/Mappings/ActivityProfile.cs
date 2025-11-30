using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using System;

namespace EduShpere.Application.Mappings
{
    public class ActivityProfile : Profile
    {
        // Use same JsonSerializerOptions as ActivitytService for consistency
        private static readonly JsonSerializerOptions RegistrationSettingsJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                .ForMember(dest => dest.numberOfSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count()))
           .ForMember(dest => dest.numberOfPendingSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count(p => p.Score == null)))
           .ForMember(dest => dest.numberOfCompletedSubmission,
               opt => opt.MapFrom(src => src.Submissions.Count(p => p.Score != null)))
                .ForMember(dest => dest.OnlyTeacherCanRegister, opt => opt.MapFrom(src => src.OnlyTeacherCanRegister ?? false))
                .ForMember(dest => dest.RegistrationReward, opt => opt.MapFrom(src => src.RegistrationReward != null && !src.RegistrationReward.IsDeleted ? src.RegistrationReward : null))
                .ForMember(dest => dest.ActivityDetail, opt => opt.MapFrom(src => src.ActivityDetail != null && !src.ActivityDetail.IsDeleted ? src.ActivityDetail : null))
                .ForMember(dest => dest.GradingSettings, opt => opt.Ignore()) // Ignore default mapping, handle in AfterMap
                .ForMember(dest => dest.RegistrationSettings, opt => opt.Ignore()) // Ignore default mapping, handle in AfterMap
                .ForMember(dest => dest.ProblemText, opt => opt.Ignore()) // Handle in AfterMap để kiểm tra thời gian mở đề
                .ForMember(dest => dest.ProblemFileUrl, opt => opt.Ignore()) // Handle in AfterMap để kiểm tra thời gian mở đề
                .ForMember(dest => dest.IsProblemVisible, opt => opt.Ignore()) // Handle in AfterMap
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

                    // Deserialize RegistrationSettings from JSON string in DB
                    // Only deserialize if RegistrationSettings exists in DB
                    if (!string.IsNullOrWhiteSpace(src.RegistrationSettings))
                    {
                        try
                        {
                            // Debug: Log raw RegistrationSettings from DB
                            System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Activity ID: {src.Id}, SubType: {src.SubType}");
                            System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Raw RegistrationSettings from DB: {src.RegistrationSettings}");
                            
                            dest.RegistrationSettings = JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(src.RegistrationSettings, RegistrationSettingsJsonOptions);
                            
                            System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Deserialized RegistrationSettings: GroupRegistration = {dest.RegistrationSettings?.GroupRegistration != null}");
                            if (dest.RegistrationSettings?.GroupRegistration != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[ActivityProfile] MinMembers from DB: {dest.RegistrationSettings.GroupRegistration.MinMembers}, MaxMembers from DB: {dest.RegistrationSettings.GroupRegistration.MaxMembers}");
                            }
                            
                            // IMPORTANT: Preserve values from DB, don't create defaults that override DB values
                            // If GroupRegistration is null after deserialization, it means DB doesn't have it - preserve null
                            // Only fix invalid MinMembers (<= 0), preserve all other values from DB
                            if (dest.RegistrationSettings != null && dest.RegistrationSettings.GroupRegistration != null)
                            {
                                // Only fix invalid MinMembers (<= 0 or default 0), preserve valid values from DB
                                if (dest.RegistrationSettings.GroupRegistration.MinMembers <= 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Fixing invalid MinMembers (was {dest.RegistrationSettings.GroupRegistration.MinMembers})");
                                    dest.RegistrationSettings.GroupRegistration.MinMembers = 1;
                                }
                                // MaxMembers can be null (unlimited) - preserve DB value
                                // RequireLeader - preserve DB value
                            }
                            // If GroupRegistration is null after deserialization, don't create default here
                            // Only create default if RegistrationSettings is null/empty in DB (handled in else block below)
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Error deserializing RegistrationSettings: {ex.Message}");
                            // If deserialization fails, only create default for CreativeContest
                            if (src.SubType == "CreativeContest")
                            {
                                dest.RegistrationSettings = CreateDefaultRegistrationSettings(src.SubType);
                            }
                            else
                            {
                                dest.RegistrationSettings = null; // Don't create defaults for non-CreativeContest
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ActivityProfile] Activity ID: {src.Id}, SubType: {src.SubType}, RegistrationSettings is null/empty");
                        // Only create default settings for CreativeContest when RegistrationSettings is null/empty
                        // For other types, return null (no group registration settings)
                        if (src.SubType == "CreativeContest")
                        {
                            System.Diagnostics.Debug.WriteLine("[ActivityProfile] Creating default RegistrationSettings for CreativeContest (null in DB)");
                            dest.RegistrationSettings = CreateDefaultRegistrationSettings(src.SubType);
                        }
                        else
                        {
                            dest.RegistrationSettings = null; // No registration settings for non-CreativeContest
                        }
                    }
                    
                    // Logic hiển thị đề bài: chỉ hiển thị sau StartDate
                    var now = DateTime.UtcNow;
                    var hasSubmission = string.Equals(src.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase) ||
                                       src.SubType?.ToLower().Contains("submission") == true ||
                                       src.SubType?.ToLower().Contains("contest") == true;
                    
                    if (hasSubmission && src.StartDate.HasValue)
                    {
                        // Kiểm tra đã đến thời gian mở đề chưa (StartDate là thời điểm mở đề)
                        var startDateUtc = src.StartDate.Value.ToUniversalTime();
                        dest.IsProblemVisible = now >= startDateUtc;
                        
                        if (dest.IsProblemVisible)
                        {
                            // Đã đến thời gian mở đề - hiển thị đầy đủ
                            dest.ProblemText = src.ProblemText;
                            dest.ProblemFileUrl = src.ProblemFileUrl;
                        }
                        else
                        {
                            // Chưa đến thời gian mở đề - ẩn đề bài
                            dest.ProblemText = null;
                            dest.ProblemFileUrl = null;
                        }
                        
                        // Luôn trả về SubmissionDeadline (không phụ thuộc thời gian)
                        dest.SubmissionDeadline = src.SubmissionDeadline;
                    }
                    else
                    {
                        // Không phải activity có submission - không có đề bài
                        dest.ProblemText = null;
                        dest.ProblemFileUrl = null;
                        dest.SubmissionDeadline = null;
                        dest.IsProblemVisible = false;
                    }
                });

            // Related entities to DTOs
            CreateMap<ActivitySpeaker, ActivitySpeakerDto>();
            CreateMap<ActivityProgram, ActivityProgramDto>();
            CreateMap<ActivitySport, ActivitySportDto>();
            CreateMap<ActivityDetail, ActivityDetailDto>();
            CreateMap<ActivityRegistrationReward, ActivityRegistrationRewardDto>();
            CreateMap<ActivityParticipant, ActivityParticipantDto>()
                .ForMember(dest => dest.ClassGroupId, opt => opt.MapFrom(src => src.ClassGroupId))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    src.User != null
                        ? (string.IsNullOrWhiteSpace(src.User.FirstName) && string.IsNullOrWhiteSpace(src.User.LastName)
                            ? src.User.Username
                            : $"{src.User.LastName} {src.User.FirstName}".Trim())
                        : null))
                .ForMember(dest => dest.UserAvatarUrl, opt => opt.MapFrom(src => src.User != null ? src.User.AvatarUrl : null))
                .ForMember(dest => dest.ClassGroupName, opt => opt.MapFrom(src => src.ClassGroup != null ? src.ClassGroup.Name : null))
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.ClassGroup != null ? src.ClassGroup.Grade : null))
                .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport != null ? src.Sport.SportName : null));
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
            CreateMap<ActivityResponseDto, Activity>();
        }

        private static ActivityRegistrationSettingsDto CreateDefaultRegistrationSettings(string? subType)
        {
            // For CreativeContest, default to group registration with minMembers = 1
            if (subType == "CreativeContest")
            {
                return new ActivityRegistrationSettingsDto
                {
                    GroupRegistration = new GroupRegistrationSettingsDto
                    {
                        MinMembers = 1,
                        MaxMembers = null, // Unlimited
                        RequireLeader = true
                    }
                };
            }

            // For other activity types, default to single registration (minMembers = 1, no max limit)
            return new ActivityRegistrationSettingsDto
            {
                GroupRegistration = new GroupRegistrationSettingsDto
                {
                    MinMembers = 1,
                    MaxMembers = null, // Unlimited
                    RequireLeader = false
                }
            };
        }
    }
}
