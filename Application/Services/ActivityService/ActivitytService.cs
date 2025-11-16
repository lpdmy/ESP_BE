using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace EduShpere.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repo;
        private readonly IActivityRuleRepository _ruleRepo;
        private readonly IActivitySpeakerRepository _speakerRepo;
        private readonly IActivityProgramRepository _programRepo;
        private readonly IActivitySportRepository _sportRepo;
        private readonly IActivityDetailRepository _detailRepo;
        private readonly IActivityRegistrationRewardRepository _registrationRewardRepo;
        private readonly IActivityRewardRepository _awardRepo;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly EduShpereDbContext _context;

        public ActivityService(
            IActivityRepository repo,
            IActivityRuleRepository ruleRepo,
            IActivitySpeakerRepository speakerRepo,
            IActivityProgramRepository programRepo,
            IActivitySportRepository sportRepo,
            IActivityDetailRepository detailRepo,
            IActivityRegistrationRewardRepository registrationRewardRepo,
            IActivityRewardRepository awardRepo,
            IMapper mapper,
            IAuditService auditService,
            EduShpereDbContext context)
        {
            _repo = repo;
            _ruleRepo = ruleRepo;
            _speakerRepo = speakerRepo;
            _programRepo = programRepo;
            _sportRepo = sportRepo;
            _detailRepo = detailRepo;
            _registrationRewardRepo = registrationRewardRepo;
            _awardRepo = awardRepo;
            _mapper = mapper;
            _auditService = auditService;
            _context = context;
        }
        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search = null)
        {
            return await _repo.GetAllWithPagingAsync(pageNumber, pageSize, search);
        }
        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdWithIncludesAsync(id);
        }
        public async Task<ActivityResponseDto> AddAsync(CreateActivityDto dto)
        {
            // Validation
            if (dto.StartDate >= dto.EndDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
            }
            if (dto.RegisterDate >= dto.EndRegisterDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDayRegister);
            }
            if (dto.EndRegisterDate >= dto.StartDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.EndDayRegisterAfterStarDay);
            }
            if (dto.MaxParticipants <= 0)
            {
                throw new BadRequestException(ErrorMessages.Activity.MaxParticipantGreaterThanZero);
            }

            // Map StarPointRewards from frontend format if provided
            if (dto.StarPointRewards != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.StarPointRewards.Registration) && 
                    int.TryParse(dto.StarPointRewards.Registration, out var regPoints))
                {
                    dto.RegistrationReward = new ActivityRegistrationRewardDto
                    {
                        StarPoints = regPoints
                    };
                }
                
                if (dto.StarPointRewards.Awards != null && dto.StarPointRewards.Awards.Any())
                {
                    dto.Awards = dto.StarPointRewards.Awards
                        .Where(a => !string.IsNullOrWhiteSpace(a.Name) && 
                                   !string.IsNullOrWhiteSpace(a.Points) && 
                                   int.TryParse(a.Points, out _))
                        .Select(a => new ActivityAwardDto
                        {
                            Name = a.Name,
                            Points = int.Parse(a.Points!),
                        })
                        .ToList();
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create main Activity entity
            var activity = new Activity
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                Category = dto.Category,
                SubType = dto.SubType,
                ThumbnailUrl = dto.ThumbnailUrl,
                    Organizer = dto.Organizer,
                RegisterDate = dto.RegisterDate,
                EndRegisterDate = dto.EndRegisterDate,
                    MaxParticipants = dto.MaxParticipants,
                    ClubId = dto.ClubId,
                // Set IsGrade flag (nullable bool)
                IsGrade = dto.GradingSettings != null && dto.GradingSettings.Criteria != null && dto.GradingSettings.Criteria.Any(),
                // Serialize only Criteria to JSON string (without Unicode escaping)
                GradingSettings = dto.GradingSettings != null && dto.GradingSettings.Criteria != null && dto.GradingSettings.Criteria.Any()
                    ? JsonSerializer.Serialize(dto.GradingSettings.Criteria, new JsonSerializerOptions 
                    { 
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                    })
                    : null,
                // Registration Settings
                OnlyTeacherCanRegister = dto.OnlyTeacherCanRegister,
                };

                _auditService.SetAuditFieldsForCreate(activity);
                await _context.Activities.AddAsync(activity);
                await _context.SaveChangesAsync(); // Save to get Activity.Id

                // Create Rules
                if (dto.Rules != null && dto.Rules.Any(r => !string.IsNullOrWhiteSpace(r)))
                {
                    var rules = dto.Rules
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(ruleText => new ActivityRule
                        {
                            ActivityId = activity.Id,
                            RuleText = ruleText.Trim(),
                        })
                        .ToList();

                    foreach (var rule in rules)
                    {
                        _auditService.SetAuditFieldsForCreate(rule);
                    }
                    await _context.ActivityRules.AddRangeAsync(rules);
                }

                // Create Sports (for SportsFestival)
                // Predefined sports list (should match frontend)
                var predefinedSports = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Chạy 100m", "Chạy 400m", "Nhảy cao", "Nhảy xa", 
                    "Ném bóng", "Bóng đá", "Bóng chuyền", "Bóng rổ"
                };
                
                if (dto.SportsCategories != null && dto.SportsCategories.Any())
                {
                    var sports = dto.SportsCategories
                        .Select(sportName => new ActivitySport
                        {
                            ActivityId = activity.Id,
                            SportName = sportName.Trim(),
                            IsCustom = !predefinedSports.Contains(sportName.Trim()),
                        })
                        .ToList();

                    foreach (var sport in sports)
                    {
                        _auditService.SetAuditFieldsForCreate(sport);
                    }
                    await _context.ActivitySports.AddRangeAsync(sports);
                }

                // Create ActivityDetail (for SportsFestival or CreativeContest)
                if ((dto.SubType == "SportsFestival" && !string.IsNullOrEmpty(dto.CompetitionType)) ||
                    (dto.SubType == "CreativeContest" && (!string.IsNullOrEmpty(dto.Theme) || !string.IsNullOrEmpty(dto.Genre))))
                {
                    var detail = new ActivityDetail
                    {
                        ActivityId = activity.Id,
                        CompetitionType = dto.CompetitionType,
                        Theme = dto.Theme,
                        Genre = dto.Genre,
                        PaperSize = dto.PaperSize,
                        DrawingMedium = dto.DrawingMedium,
                        TimeLimit = dto.TimeLimit,
                        SubmissionFormat = dto.SubmissionFormat,
                    };

                    _auditService.SetAuditFieldsForCreate(detail);
                    await _context.ActivityDetails.AddAsync(detail);
                }

                // Create Speakers (for SeminarWorkshop)
                if (dto.Speakers != null && dto.Speakers.Any())
                {
                    var speakers = dto.Speakers
                        .Select((speakerDto, index) => new ActivitySpeaker
                        {
                            ActivityId = activity.Id,
                            Name = speakerDto.Name.Trim(),
                            Title = speakerDto.Title?.Trim(),
                            Bio = speakerDto.Bio?.Trim(),
                            ImageUrl = speakerDto.ImageUrl?.Trim(),
                            Order = index,
                        })
                        .ToList();

                    foreach (var speaker in speakers)
                    {
                        _auditService.SetAuditFieldsForCreate(speaker);
                    }
                    await _context.ActivitySpeakers.AddRangeAsync(speakers);
                }

                // Create Programs (for SeminarWorkshop)
                if (dto.ProgramItems != null && dto.ProgramItems.Any())
                {
                    var programs = dto.ProgramItems
                        .Select((programDto, index) => new ActivityProgram
                        {
                            ActivityId = activity.Id,
                            Title = programDto.Title.Trim(),
                            Time = programDto.Time?.Trim(),
                            Description = programDto.Description?.Trim(),
                            Order = index,
                        })
                        .ToList();

                    foreach (var program in programs)
                    {
                        _auditService.SetAuditFieldsForCreate(program);
                    }
                    await _context.ActivityPrograms.AddRangeAsync(programs);
                }

                // Create RegistrationReward
                if (dto.RegistrationReward != null && dto.RegistrationReward.StarPoints > 0)
                {
                    var registrationReward = new ActivityRegistrationReward
                    {
                        ActivityId = activity.Id,
                        StarPoints = dto.RegistrationReward.StarPoints,
                    };

                    _auditService.SetAuditFieldsForCreate(registrationReward);
                    await _context.ActivityRegistrationRewards.AddAsync(registrationReward);
                }

                // Create Awards (ActivityReward)
                if (dto.Awards != null && dto.Awards.Any(a => (!string.IsNullOrWhiteSpace(a.Name) || !string.IsNullOrWhiteSpace(a.Rank)) && (a.StarPoints > 0 || a.Points > 0)))
                {
                    var awards = dto.Awards
                        .Where(a => (!string.IsNullOrWhiteSpace(a.Name) || !string.IsNullOrWhiteSpace(a.Rank)) && (a.StarPoints > 0 || a.Points > 0))
                        .Select(awardDto => new ActivityReward
                        {
                            ActivityId = activity.Id,
                            Rank = awardDto.Name?.Trim() ?? awardDto.Rank?.Trim(),
                            StarPoints = awardDto.StarPoints > 0 ? awardDto.StarPoints : awardDto.Points,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                        })
                        .ToList();

                    await _context.ActivityRewards.AddRangeAsync(awards);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with all includes
                var activityWithIncludes = await _repo.GetByIdWithIncludesAsync(activity.Id);
                return _mapper.Map<ActivityResponseDto>(activityWithIncludes);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<ActivityResponseDto> UpdateAsync(UpdateActivityDto dto)
        {
            var existingActivity = await _repo.GetByIdWithIncludesAsync(dto.Id);
            if (existingActivity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Validation
            var startDate = dto.StartDate ?? existingActivity.StartDate;
            var endDate = dto.EndDate ?? existingActivity.EndDate;
            var registerDate = dto.RegisterDate ?? (DateTime?)existingActivity.RegisterDate;
            var endRegisterDate = dto.EndRegisterDate ?? (DateTime?)existingActivity.EndRegisterDate;

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
            }
            if (registerDate.HasValue && endRegisterDate.HasValue && registerDate >= endRegisterDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDayRegister);
            }
            if (endRegisterDate.HasValue && startDate.HasValue && endRegisterDate >= startDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.EndDayRegisterAfterStarDay);
            }

            // Map StarPointRewards from frontend format if provided
            if (dto.StarPointRewards != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.StarPointRewards.Registration) && 
                    int.TryParse(dto.StarPointRewards.Registration, out var regPoints))
                {
                    dto.RegistrationReward = new ActivityRegistrationRewardDto
                    {
                        StarPoints = regPoints
                    };
                }
                
                if (dto.StarPointRewards.Awards != null && dto.StarPointRewards.Awards.Any())
                {
                    dto.Awards = dto.StarPointRewards.Awards
                        .Where(a => !string.IsNullOrWhiteSpace(a.Name) && 
                                   !string.IsNullOrWhiteSpace(a.Points) && 
                                   int.TryParse(a.Points, out _))
                        .Select(a => new ActivityAwardDto
                        {
                            Name = a.Name,
                            Points = int.Parse(a.Points!),
                        })
                        .ToList();
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update main Activity entity
                if (dto.Title != null) existingActivity.Title = dto.Title;
                if (dto.Description != null) existingActivity.Description = dto.Description;
                if (dto.StartDate.HasValue) existingActivity.StartDate = dto.StartDate;
                if (dto.EndDate.HasValue) existingActivity.EndDate = dto.EndDate;
                if (dto.Location != null) existingActivity.Location = dto.Location;
                if (dto.Category.HasValue) existingActivity.Category = dto.Category.Value;
                if (dto.SubType != null) existingActivity.SubType = dto.SubType;
                if (dto.ThumbnailUrl != null) existingActivity.ThumbnailUrl = dto.ThumbnailUrl;
                if (dto.Organizer != null) existingActivity.Organizer = dto.Organizer;
                if (dto.MaxParticipants.HasValue) existingActivity.MaxParticipants = dto.MaxParticipants.Value;
                if (dto.RegisterDate.HasValue) existingActivity.RegisterDate = dto.RegisterDate.Value;
                if (dto.EndRegisterDate.HasValue) existingActivity.EndRegisterDate = dto.EndRegisterDate.Value;
                if (dto.ClubId.HasValue) existingActivity.ClubId = dto.ClubId;
                
                // Update IsGrade flag (nullable bool)
                if (dto.GradingSettings != null)
                {
                    existingActivity.IsGrade = dto.GradingSettings.Criteria != null && dto.GradingSettings.Criteria.Any();
                    // Serialize only Criteria to JSON string (without Unicode escaping)
                    existingActivity.GradingSettings = dto.GradingSettings.Criteria != null && dto.GradingSettings.Criteria.Any()
                        ? JsonSerializer.Serialize(dto.GradingSettings.Criteria, new JsonSerializerOptions 
                        { 
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                        })
                        : null;
                }
                
                // Update Registration Settings
                if (dto.OnlyTeacherCanRegister.HasValue)
                {
                    existingActivity.OnlyTeacherCanRegister = dto.OnlyTeacherCanRegister.Value;
                }

                _auditService.SetAuditFieldsForUpdate(existingActivity);
                _context.Activities.Update(existingActivity);

                // Update Rules - Delete old and create new
                if (dto.Rules != null)
                {
                    var existingRules = await _context.ActivityRules
                        .Where(r => r.ActivityId == existingActivity.Id && !r.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var rule in existingRules)
                    {
                        rule.IsDeleted = true;
                        _auditService.SetAuditFieldsForDelete(rule);
                    }
                    
                    var rules = dto.Rules
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(ruleText => new ActivityRule
                        {
                            ActivityId = existingActivity.Id,
                            RuleText = ruleText.Trim(),
                        })
                        .ToList();

                    foreach (var rule in rules)
                    {
                        _auditService.SetAuditFieldsForCreate(rule);
                    }
                    if (rules.Any())
                    {
                        await _context.ActivityRules.AddRangeAsync(rules);
                    }
                }

                // Update Sports - Delete old and create new
                // Predefined sports list (should match frontend)
                var predefinedSports = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Chạy 100m", "Chạy 400m", "Nhảy cao", "Nhảy xa", 
                    "Ném bóng", "Bóng đá", "Bóng chuyền", "Bóng rổ"
                };
                
                if (dto.SportsCategories != null)
                {
                    var existingSports = await _context.ActivitySports
                        .Where(s => s.ActivityId == existingActivity.Id && !s.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var sport in existingSports)
                    {
                        sport.IsDeleted = true;
                        _auditService.SetAuditFieldsForDelete(sport);
                    }
                    
                    var sports = dto.SportsCategories
                        .Select(sportName => new ActivitySport
                        {
                            ActivityId = existingActivity.Id,
                            SportName = sportName.Trim(),
                            IsCustom = !predefinedSports.Contains(sportName.Trim()),
                        })
                        .ToList();

                    foreach (var sport in sports)
                    {
                        _auditService.SetAuditFieldsForCreate(sport);
                    }
                    if (sports.Any())
                    {
                        await _context.ActivitySports.AddRangeAsync(sports);
                    }
                }

                // Update ActivityDetail
                if (dto.SubType == "SportsFestival" || dto.SubType == "CreativeContest")
                {
                    var existingDetail = await _detailRepo.GetByActivityIdAsync(existingActivity.Id);
                    
                    if (existingDetail != null)
                    {
                        if (dto.CompetitionType != null) existingDetail.CompetitionType = dto.CompetitionType;
                        if (dto.Theme != null) existingDetail.Theme = dto.Theme;
                        if (dto.Genre != null) existingDetail.Genre = dto.Genre;
                        if (dto.PaperSize != null) existingDetail.PaperSize = dto.PaperSize;
                        if (dto.DrawingMedium != null) existingDetail.DrawingMedium = dto.DrawingMedium;
                        if (dto.TimeLimit != null) existingDetail.TimeLimit = dto.TimeLimit;
                        if (dto.SubmissionFormat != null) existingDetail.SubmissionFormat = dto.SubmissionFormat;
                        
                        _auditService.SetAuditFieldsForUpdate(existingDetail);
                        _context.ActivityDetails.Update(existingDetail);
                    }
                    else if (dto.CompetitionType != null || dto.Theme != null || dto.Genre != null)
                    {
                        var detail = new ActivityDetail
                        {
                            ActivityId = existingActivity.Id,
                            CompetitionType = dto.CompetitionType,
                            Theme = dto.Theme,
                            Genre = dto.Genre,
                            PaperSize = dto.PaperSize,
                            DrawingMedium = dto.DrawingMedium,
                            TimeLimit = dto.TimeLimit,
                            SubmissionFormat = dto.SubmissionFormat,
                        };

                        _auditService.SetAuditFieldsForCreate(detail);
                        await _context.ActivityDetails.AddAsync(detail);
                    }
                }

                // Update Speakers - Delete old and create new
                if (dto.Speakers != null)
                {
                    var existingSpeakers = await _context.ActivitySpeakers
                        .Where(s => s.ActivityId == existingActivity.Id && !s.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var speaker in existingSpeakers)
                    {
                        speaker.IsDeleted = true;
                        _auditService.SetAuditFieldsForDelete(speaker);
                    }
                    
                    var speakers = dto.Speakers
                        .Select((speakerDto, index) => new ActivitySpeaker
                        {
                            ActivityId = existingActivity.Id,
                            Name = speakerDto.Name.Trim(),
                            Title = speakerDto.Title?.Trim(),
                            Bio = speakerDto.Bio?.Trim(),
                            ImageUrl = speakerDto.ImageUrl?.Trim(),
                            Order = index,
                        })
                        .ToList();

                    foreach (var speaker in speakers)
                    {
                        _auditService.SetAuditFieldsForCreate(speaker);
                    }
                    if (speakers.Any())
                    {
                        await _context.ActivitySpeakers.AddRangeAsync(speakers);
                    }
                }

                // Update Programs - Delete old and create new
                if (dto.ProgramItems != null)
                {
                    var existingPrograms = await _context.ActivityPrograms
                        .Where(p => p.ActivityId == existingActivity.Id && !p.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var program in existingPrograms)
                    {
                        program.IsDeleted = true;
                        _auditService.SetAuditFieldsForDelete(program);
                    }
                    
                    var programs = dto.ProgramItems
                        .Select((programDto, index) => new ActivityProgram
                        {
                            ActivityId = existingActivity.Id,
                            Title = programDto.Title.Trim(),
                            Time = programDto.Time?.Trim(),
                            Description = programDto.Description?.Trim(),
                            Order = index,
                        })
                        .ToList();

                    foreach (var program in programs)
                    {
                        _auditService.SetAuditFieldsForCreate(program);
                    }
                    if (programs.Any())
                    {
                        await _context.ActivityPrograms.AddRangeAsync(programs);
                    }
                }

                // Update RegistrationReward
                if (dto.RegistrationReward != null)
                {
                    var existingReward = await _context.ActivityRegistrationRewards
                        .FirstOrDefaultAsync(r => r.ActivityId == existingActivity.Id && !r.IsDeleted);
                    
                    if (existingReward != null)
                    {
                        existingReward.StarPoints = dto.RegistrationReward.StarPoints;
                        _auditService.SetAuditFieldsForUpdate(existingReward);
                        _context.ActivityRegistrationRewards.Update(existingReward);
                    }
                    else if (dto.RegistrationReward.StarPoints > 0)
                    {
                        var registrationReward = new ActivityRegistrationReward
                        {
                            ActivityId = existingActivity.Id,
                            StarPoints = dto.RegistrationReward.StarPoints,
                        };

                        _auditService.SetAuditFieldsForCreate(registrationReward);
                        await _context.ActivityRegistrationRewards.AddAsync(registrationReward);
                    }
                }

                // Update Awards - Delete old and create new
                if (dto.Awards != null)
                {
                    var existingAwards = await _context.ActivityRewards
                        .Where(r => r.ActivityId == existingActivity.Id && !r.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var award in existingAwards)
                    {
                        award.IsDeleted = true;
                    }
                    
                    var awards = dto.Awards
                        .Where(a => (!string.IsNullOrWhiteSpace(a.Name) || !string.IsNullOrWhiteSpace(a.Rank)) && (a.StarPoints > 0 || a.Points > 0))
                        .Select(awardDto => new ActivityReward
                        {
                            ActivityId = existingActivity.Id,
                            Rank = awardDto.Name?.Trim() ?? awardDto.Rank?.Trim(),
                            StarPoints = awardDto.StarPoints > 0 ? awardDto.StarPoints : awardDto.Points,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                        })
                        .ToList();

                    if (awards.Any())
                    {
                        await _context.ActivityRewards.AddRangeAsync(awards);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with all includes
                var activityWithIncludes = await _repo.GetByIdWithIncludesAsync(existingActivity.Id);
                return _mapper.Map<ActivityResponseDto>(activityWithIncludes);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
