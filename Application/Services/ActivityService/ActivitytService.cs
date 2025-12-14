using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Linq;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

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
        private static readonly JsonSerializerOptions RegistrationSettingsJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
            var (items, totalCount) = await _repo.GetAllWithPagingAsync(pageNumber, pageSize, search);
            return (items ?? Enumerable.Empty<Activity>(), totalCount);
        }

        /// <summary>
        /// Lấy danh sách activities tối ưu cho list view - chỉ trả về các field cần thiết, không load navigation properties
        /// </summary>
        public async Task<IEnumerable<ActivityListItemDto>> GetListItemsAsync()
        {
            // Query tối ưu: chỉ select các field cần thiết, không include navigation properties
            // Lưu ý: Không thể deserialize JSON trong LINQ query, nên load raw data trước
            var activitiesData = await _context.Activities
                .Where(a => !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Description,
                    a.StartDate,
                    a.EndDate,
                    a.Location,
                    a.Organizer,
                    a.MaxParticipants,
                    a.Category,
                    a.SubType,
                    a.ThumbnailUrl,
                    a.RegisterDate,
                    a.EndRegisterDate,
                    a.RegistrationSettings
                })
                .ToListAsync();

            // Deserialize và map sang DTO sau khi đã load từ DB (tránh memory leak warning)
            var jsonOptions = RegistrationSettingsJsonOptions;
            var activities = activitiesData.Select(a => new ActivityListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Location = a.Location,
                Organizer = a.Organizer,
                MaxParticipants = a.MaxParticipants,
                Category = a.Category,
                SubType = a.SubType,
                ThumbnailUrl = a.ThumbnailUrl,
                RegisterDate = a.RegisterDate,
                EndRegisterDate = a.EndRegisterDate,
                RegistrationSettings = !string.IsNullOrEmpty(a.RegistrationSettings)
                    ? JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(a.RegistrationSettings, jsonOptions)
                    : null,
                // Tính status dựa trên ngày tháng (sử dụng UTC để đảm bảo consistency)
                Status = a.StartDate.HasValue && a.EndDate.HasValue
                    ? (DateTime.UtcNow < a.StartDate.Value.ToUniversalTime()
                        ? "Sắp diễn ra"
                        : DateTime.UtcNow >= a.StartDate.Value.ToUniversalTime() && DateTime.UtcNow <= a.EndDate.Value.ToUniversalTime()
                        ? "Đang diễn ra"
                        : "Đã kết thúc")
                    : "Đang cập nhật"
            }).ToList();

            // Tính số lượng participants cho mỗi activity (batch query để tối ưu)
            var activityIds = activities.Select(a => a.Id).ToList();
            var participantCounts = await _context.ActivityParticipants
                .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                .GroupBy(p => p.ActivityId)
                .Select(g => new { ActivityId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ActivityId, x => x.Count);

            // Gán số lượng participants
            foreach (var activity in activities)
            {
                activity.NumberOfParticipants = participantCounts.GetValueOrDefault(activity.Id, 0);
            }

            return activities;
        }

        /// <summary>
        /// Lấy danh sách activities với filtering, paging và sorting tối ưu
        /// Mặc định sắp xếp theo StartDate DESC (hoạt động gần nhất)
        /// </summary>
        public async Task<PaginationResponseDto<ActivityListItemDto>> GetListItemsWithFilterAsync(ActivityListFilterDto filter, int? userId = null)
        {
            // Validation được xử lý bởi PaginationRequestDto attributes
            // Set defaults nếu cần
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 30;
            if (filter.PageSize > 100) filter.PageSize = 100; // Limit max page size

            // Build base query với filtering
            var query = _context.Activities
                .Where(a => !a.IsDeleted)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchLower))
                );
            }

            // Apply SubType filter
            if (!string.IsNullOrWhiteSpace(filter.SubType) && filter.SubType.ToLower() != "all")
            {
                query = query.Where(a => a.SubType == filter.SubType);
            }

            // Apply Status filter (upcoming, ongoing, ended)
            // Sử dụng UTC time để đảm bảo consistency
            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status.ToLower() != "all")
            {
                var now = DateTime.UtcNow;
                query = filter.Status.ToLower() switch
                {
                    "upcoming" => query.Where(a => 
                        a.StartDate.HasValue && 
                        a.StartDate.Value.ToUniversalTime() > now),
                    "ongoing" => query.Where(a => 
                        a.StartDate.HasValue && 
                        a.EndDate.HasValue && 
                        a.StartDate.Value.ToUniversalTime() <= now && 
                        a.EndDate.Value.ToUniversalTime() >= now),
                    "ended" => query.Where(a => 
                        a.EndDate.HasValue && 
                        a.EndDate.Value.ToUniversalTime() < now),
                    _ => query
                };
            }

            // Apply DateFrom filter
            // Convert to UTC để đảm bảo consistency
            if (filter.DateFrom.HasValue)
            {
                var dateFromUtc = filter.DateFrom.Value.ToUniversalTime();
                query = query.Where(a => 
                    a.StartDate.HasValue && 
                    a.StartDate.Value.ToUniversalTime() >= dateFromUtc);
            }

            // Apply DateTo filter
            // Convert to UTC và set to end of day để include cả ngày đó
            if (filter.DateTo.HasValue)
            {
                var dateToUtc = filter.DateTo.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(a => 
                    a.EndDate.HasValue && 
                    a.EndDate.Value.ToUniversalTime() <= dateToUtc);
            }

            // Apply Organizer filter
            if (!string.IsNullOrWhiteSpace(filter.Organizer))
            {
                var organizerLower = filter.Organizer.ToLower();
                query = query.Where(a => a.Organizer != null && a.Organizer.ToLower().Contains(organizerLower));
            }

            // Apply MinParticipants and MaxParticipants filters using subquery BEFORE pagination
            // This ensures accurate pagination results
            if (filter.MinParticipants.HasValue || filter.MaxParticipants.HasValue)
            {
                var minParticipants = filter.MinParticipants ?? 0;
                var maxParticipants = filter.MaxParticipants ?? int.MaxValue;

                // Filter by participant count using subquery in EF Core
                // This will be translated to SQL subquery for efficiency
                query = query.Where(a =>
                    _context.ActivityParticipants
                        .Count(p => p.ActivityId == a.Id && !p.IsDeleted) >= minParticipants &&
                    _context.ActivityParticipants
                        .Count(p => p.ActivityId == a.Id && !p.IsDeleted) <= maxParticipants
                );
            }

            // Get total count AFTER all filters (including participant count filter)
            var totalCount = await query.CountAsync();

            // Apply sorting
            // SortBy và SortDescending đã được kế thừa từ PaginationRequestDto
            var sortBy = filter.SortBy?.ToLower() ?? "startdate";
            var sortOrder = filter.SortDescending ? "DESC" : "ASC";

            query = sortBy switch
            {
                "createdat" => sortOrder == "ASC" 
                    ? query.OrderBy(a => a.CreatedAt)
                    : query.OrderByDescending(a => a.CreatedAt),
                "title" => sortOrder == "ASC"
                    ? query.OrderBy(a => a.Title)
                    : query.OrderByDescending(a => a.Title),
                _ => sortOrder == "ASC" // Default: StartDate
                    ? query.OrderBy(a => a.StartDate ?? DateTime.MaxValue)
                    : query.OrderByDescending(a => a.StartDate ?? DateTime.MinValue)
            };

            // Apply paging
            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var activitiesData = await query
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Description,
                    a.StartDate,
                    a.EndDate,
                    a.Location,
                    a.Organizer,
                    a.MaxParticipants,
                    a.Category,
                    a.SubType,
                    a.ThumbnailUrl,
                    a.RegisterDate,
                    a.EndRegisterDate,
                    a.RegistrationSettings,
                    a.CreatedAt
                })
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            // Deserialize và map sang DTO
            var jsonOptions = RegistrationSettingsJsonOptions;
            var activities = activitiesData.Select(a => new ActivityListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Location = a.Location,
                Organizer = a.Organizer,
                MaxParticipants = a.MaxParticipants,
                Category = a.Category,
                SubType = a.SubType,
                ThumbnailUrl = a.ThumbnailUrl,
                RegisterDate = a.RegisterDate,
                EndRegisterDate = a.EndRegisterDate,
                RegistrationSettings = !string.IsNullOrEmpty(a.RegistrationSettings)
                    ? JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(a.RegistrationSettings, jsonOptions)
                    : null,
                // Tính status dựa trên ngày tháng (sử dụng UTC để đảm bảo consistency)
                Status = a.StartDate.HasValue && a.EndDate.HasValue
                    ? (DateTime.UtcNow < a.StartDate.Value.ToUniversalTime()
                        ? "Sắp diễn ra"
                        : DateTime.UtcNow >= a.StartDate.Value.ToUniversalTime() && DateTime.UtcNow <= a.EndDate.Value.ToUniversalTime()
                        ? "Đang diễn ra"
                        : "Đã kết thúc")
                    : "Đang cập nhật"
            }).ToList();

            // Tính số lượng participants cho mỗi activity (batch query để tối ưu)
            var activityIds = activities.Select(a => a.Id).ToList();
            if (activityIds.Any())
            {
                var participantCounts = await _context.ActivityParticipants
                    .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                    .GroupBy(p => p.ActivityId)
                    .Select(g => new { ActivityId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ActivityId, x => x.Count);

                // Check if user is registered for each activity (if userId provided)
                // Query tất cả participants của user trong các activities này
                var userRegisteredActivityIds = new HashSet<int>();
                if (userId.HasValue && userId.Value > 0 && activityIds.Any())
                {
                    // Query đơn giản và rõ ràng: tìm tất cả ActivityParticipants của user trong các activities này
                    var userParticipants = await _context.ActivityParticipants
                        .Where(p => 
                            activityIds.Contains(p.ActivityId) && 
                            p.UserId == userId.Value && 
                            !p.IsDeleted)
                        .Select(p => p.ActivityId)
                        .Distinct()
                        .ToListAsync();
                    
                    userRegisteredActivityIds = userParticipants.ToHashSet();
                }

                // Gán số lượng participants và IsRegistered
                foreach (var activity in activities)
                {
                    activity.NumberOfParticipants = participantCounts.GetValueOrDefault(activity.Id, 0);
                    activity.IsRegistered = userRegisteredActivityIds.Contains(activity.Id);
                }
            }

            return new PaginationResponseDto<ActivityListItemDto>
            {
                Data = activities,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdWithIncludesAsync(id);
        }
        public async Task<ActivityResponseDto> AddAsync(CreateActivityDto dto)
        {
            // Validation
            // Cho phép ngày bắt đầu bằng ngày kết thúc
            if (dto.StartDate > dto.EndDate)
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
            // Validate MaxParticipants: nếu có giá trị thì phải > 0, null = không giới hạn (áp dụng cho tất cả loại activity)
            if (dto.MaxParticipants.HasValue && dto.MaxParticipants.Value <= 0)
            {
                throw new BadRequestException(ErrorMessages.Activity.MaxParticipantGreaterThanZero);
            }

            // Validation cho SubmissionDeadline (chỉ áp dụng cho Activity có nộp bài)
            if (dto.SubmissionDeadline.HasValue)
            {
                // Kiểm tra SubType có phải là CreativeContest hoặc có submission không
                var hasSubmission = string.Equals(dto.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase) ||
                                   dto.SubType?.ToLower().Contains("submission") == true ||
                                   dto.SubType?.ToLower().Contains("contest") == true;
                
                if (hasSubmission)
                {
                    // SubmissionDeadline phải >= StartDate
                    if (dto.SubmissionDeadline.Value < dto.StartDate)
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineBeforeStartDate);
                    }
                    
                    // SubmissionDeadline phải <= EndDate
                    if (dto.SubmissionDeadline.Value > dto.EndDate)
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineAfterEndDate);
                    }
                }
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

            ActivityResponseDto? createdActivity = null;
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create main Activity entity
            // Ensure all dates are in UTC before saving
            var activity = new Activity
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc) 
                    : dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc) 
                    : dto.EndDate.ToUniversalTime(),
                Location = dto.Location,
                Category = dto.Category,
                SubType = dto.SubType,
                ThumbnailUrl = dto.ThumbnailUrl,
                    Organizer = dto.Organizer,
                RegisterDate = dto.RegisterDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.RegisterDate, DateTimeKind.Utc) 
                    : dto.RegisterDate.ToUniversalTime(),
                EndRegisterDate = dto.EndRegisterDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.EndRegisterDate, DateTimeKind.Utc) 
                    : dto.EndRegisterDate.ToUniversalTime(),
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
                RegistrationSettings = SerializeRegistrationSettings(dto.RegistrationSettings),
                // Problem/Submission fields
                ProblemText = dto.ProblemText,
                ProblemFileUrl = dto.ProblemFileUrl,
                SubmissionDeadline = dto.SubmissionDeadline.HasValue
                    ? (dto.SubmissionDeadline.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(dto.SubmissionDeadline.Value, DateTimeKind.Utc)
                        : dto.SubmissionDeadline.Value.ToUniversalTime())
                    : null,
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
                
                var sportsConfigurations = dto.SportsConfigurations != null && dto.SportsConfigurations.Any()
                    ? dto.SportsConfigurations
                    : dto.SportsCategories?.Select(name => new ActivitySportConfigDto { SportName = name }).ToList();

                if (sportsConfigurations != null && sportsConfigurations.Any())
                {
                    var sports = BuildSportEntities(activity.Id, sportsConfigurations, predefinedSports);

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
                
                // Reload with all includes BEFORE committing transaction
                // This ensures the query runs within the transaction scope
                var activityWithIncludes = await _repo.GetByIdWithIncludesAsync(activity.Id);
                
                await transaction.CommitAsync();
                
                    createdActivity = _mapper.Map<ActivityResponseDto>(activityWithIncludes);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            });

            return createdActivity!;
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

            var now = DateTime.UtcNow;

            // Helper function: Kiểm tra xem ngày đã qua chưa (chỉ so sánh ngày, bỏ qua giờ)
            bool IsDatePassed(DateTime? date)
            {
                if (!date.HasValue) return false;
                var dateOnly = date.Value.Date;
                var nowOnly = now.Date;
                return dateOnly <= nowOnly;
            }

            // Chỉ validate nếu ngày chưa qua
            // Cho phép ngày bắt đầu bằng ngày kết thúc
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                // Chỉ validate nếu ít nhất một trong hai ngày chưa qua
                if (!IsDatePassed(startDate) && !IsDatePassed(endDate))
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
                }
            }
            if (registerDate.HasValue && endRegisterDate.HasValue && registerDate >= endRegisterDate)
            {
                // Chỉ validate nếu ít nhất một trong hai ngày chưa qua
                if (!IsDatePassed(registerDate) && !IsDatePassed(endRegisterDate))
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDayRegister);
                }
            }
            if (endRegisterDate.HasValue && startDate.HasValue && endRegisterDate >= startDate)
            {
                // Chỉ validate nếu ít nhất một trong hai ngày chưa qua
                if (!IsDatePassed(endRegisterDate) && !IsDatePassed(startDate))
            {
                throw new BadRequestException(ErrorMessages.Activity.EndDayRegisterAfterStarDay);
                }
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

            ActivityResponseDto? updatedActivity = null;
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update main Activity entity
                // Ensure all dates are in UTC before saving
                if (dto.Title != null) existingActivity.Title = dto.Title;
                if (dto.Description != null) existingActivity.Description = dto.Description;
                if (dto.StartDate.HasValue)
                {
                    existingActivity.StartDate = dto.StartDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) 
                        : dto.StartDate.Value.ToUniversalTime();
                }
                if (dto.EndDate.HasValue)
                {
                    existingActivity.EndDate = dto.EndDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) 
                        : dto.EndDate.Value.ToUniversalTime();
                }
                if (dto.Location != null) existingActivity.Location = dto.Location;
                if (dto.Category.HasValue) existingActivity.Category = dto.Category.Value;
                if (dto.SubType != null) existingActivity.SubType = dto.SubType;
                if (dto.ThumbnailUrl != null) existingActivity.ThumbnailUrl = dto.ThumbnailUrl;
                if (dto.Organizer != null) existingActivity.Organizer = dto.Organizer;
                // Update MaxParticipants: null = không giới hạn, có giá trị = giới hạn số người
                // Có thể cập nhật cho tất cả các loại activity
                if (dto.MaxParticipants.HasValue)
                {
                    existingActivity.MaxParticipants = dto.MaxParticipants.Value;
                }
                else
                {
                    // Nếu MaxParticipants là null, set thành null (không giới hạn)
                    existingActivity.MaxParticipants = null;
                }
                if (dto.RegisterDate.HasValue)
                {
                    existingActivity.RegisterDate = dto.RegisterDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dto.RegisterDate.Value, DateTimeKind.Utc) 
                        : dto.RegisterDate.Value.ToUniversalTime();
                }
                if (dto.EndRegisterDate.HasValue)
                {
                    existingActivity.EndRegisterDate = dto.EndRegisterDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dto.EndRegisterDate.Value, DateTimeKind.Utc) 
                        : dto.EndRegisterDate.Value.ToUniversalTime();
                }
                if (dto.ClubId.HasValue) existingActivity.ClubId = dto.ClubId;
                
                // Validation cho SubmissionDeadline (chỉ áp dụng cho Activity có nộp bài)
                if (dto.SubmissionDeadline.HasValue)
                {
                    var finalStartDate = dto.StartDate ?? existingActivity.StartDate;
                    var finalEndDate = dto.EndDate ?? existingActivity.EndDate;
                    
                    // Chỉ validate nếu các ngày chưa qua
                    if (finalStartDate.HasValue && dto.SubmissionDeadline.Value < finalStartDate.Value)
                    {
                        // Chỉ validate nếu ít nhất một trong hai ngày chưa qua
                        if (!IsDatePassed(finalStartDate) && !IsDatePassed(dto.SubmissionDeadline))
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineBeforeStartDate);
                        }
                    }
                    
                    if (finalEndDate.HasValue && dto.SubmissionDeadline.Value > finalEndDate.Value)
                    {
                        // Chỉ validate nếu ít nhất một trong hai ngày chưa qua
                        if (!IsDatePassed(finalEndDate) && !IsDatePassed(dto.SubmissionDeadline))
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineAfterEndDate);
                        }
                    }
                    
                    existingActivity.SubmissionDeadline = dto.SubmissionDeadline.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(dto.SubmissionDeadline.Value, DateTimeKind.Utc)
                        : dto.SubmissionDeadline.Value.ToUniversalTime();
                }
                else if (dto.SubmissionDeadline == null && dto.SubType != null)
                {
                    // Nếu SubmissionDeadline được set thành null (xóa), chỉ xóa nếu không phải CreativeContest
                    var hasSubmission = string.Equals(dto.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase);
                    if (!hasSubmission)
                    {
                        existingActivity.SubmissionDeadline = null;
                    }
                }
                
                // Update ProblemText và ProblemFileUrl
                if (dto.ProblemText != null) existingActivity.ProblemText = dto.ProblemText;
                if (dto.ProblemFileUrl != null) existingActivity.ProblemFileUrl = dto.ProblemFileUrl;
                
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

                if (dto.RegistrationSettings != null)
                {
                    existingActivity.RegistrationSettings = SerializeRegistrationSettings(dto.RegistrationSettings);
                }

                // Update IsDeleted flag for soft delete
                if (dto.IsDeleted.HasValue)
                {
                    existingActivity.IsDeleted = dto.IsDeleted.Value;
                    if (dto.IsDeleted.Value)
                    {
                        _auditService.SetAuditFieldsForDelete(existingActivity);
                    }
                    else
                    {
                        _auditService.SetAuditFieldsForUpdate(existingActivity);
                    }
                }
                else
                {
                    _auditService.SetAuditFieldsForUpdate(existingActivity);
                }
                
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
                
                var hasSportsPayload = (dto.SportsConfigurations != null && dto.SportsConfigurations.Any()) ||
                                       (dto.SportsCategories != null && dto.SportsCategories.Any());

                if (hasSportsPayload)
                {
                    var existingSports = await _context.ActivitySports
                        .Where(s => s.ActivityId == existingActivity.Id && !s.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var sport in existingSports)
                    {
                        sport.IsDeleted = true;
                        _auditService.SetAuditFieldsForDelete(sport);
                    }

                    var sportsConfigurations = dto.SportsConfigurations != null && dto.SportsConfigurations.Any()
                        ? dto.SportsConfigurations
                        : dto.SportsCategories!.Select(name => new ActivitySportConfigDto { SportName = name }).ToList();
                    
                    var sports = BuildSportEntities(existingActivity.Id, sportsConfigurations, predefinedSports);

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
                
                // Reload with all includes BEFORE committing transaction
                // This ensures the query runs within the transaction scope
                var activityWithIncludes = await _repo.GetByIdWithIncludesAsync(existingActivity.Id);
                
                await transaction.CommitAsync();
                
                    updatedActivity = _mapper.Map<ActivityResponseDto>(activityWithIncludes);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            });

            return updatedActivity!;
        }

        private static string? SerializeRegistrationSettings(ActivityRegistrationSettingsDto? settings)
        {
            if (settings == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(settings, RegistrationSettingsJsonOptions);
        }

        private static List<ActivitySport> BuildSportEntities(int activityId, IEnumerable<ActivitySportConfigDto> configs, HashSet<string> predefinedSports)
        {
            return configs
                .Where(config => !string.IsNullOrWhiteSpace(config.SportName))
                .Select(config => new ActivitySport
                {
                    ActivityId = activityId,
                    SportName = config.SportName.Trim(),
                    MaxMembers = config.MaxMembers,
                    IsCustom = !predefinedSports.Contains(config.SportName.Trim())
                })
                .ToList();
        }

        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetActivitiesByUserIdAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null)
        {
            return await _repo.GetActivitiesByUserIdAsync(userId, pageNumber, pageSize, search, status);
        }

        public async Task<RecentActivityInputsDto> GetRecentInputsAsync(int userId, int take = 5)
        {
            // Lấy một số bản ghi gần nhất để trích xuất giá trị gợi ý, tránh load toàn bộ
            const int queryTake = 50;

            var recentItems = await _context.Activities
                .Where(a => !a.IsDeleted && a.CreatedBy == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Location,
                    a.Organizer
                })
                .Take(queryTake)
                .ToListAsync();

            static List<string> BuildDistinct(List<string?> source, int max)
            {
                return source
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(max)
                    .ToList();
            }

            var locations = BuildDistinct(recentItems.Select(r => r.Location).ToList(), take);
            var organizers = BuildDistinct(recentItems.Select(r => r.Organizer).ToList(), take);

            return new RecentActivityInputsDto
            {
                Locations = locations,
                Organizers = organizers,
                Contacts = new List<string>() // Hiện chưa lưu contact trong Activity, trả về danh sách trống
            };
        }

        public async Task<ActivityStatisticsDto> GetStatisticsAsync()
        {
            var now = DateTime.UtcNow;

            // Lấy tất cả activities chưa bị xóa
            var allActivities = await _context.Activities
                .Where(a => !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    a.StartDate,
                    a.EndDate
                })
                .ToListAsync();

            // Tính số lượng activities theo trạng thái
            var ongoingCount = allActivities.Count(a =>
                a.StartDate.HasValue &&
                a.EndDate.HasValue &&
                a.StartDate.Value.ToUniversalTime() <= now &&
                a.EndDate.Value.ToUniversalTime() >= now);

            var upcomingCount = allActivities.Count(a =>
                a.StartDate.HasValue &&
                a.StartDate.Value.ToUniversalTime() > now);

            var completedCount = allActivities.Count(a =>
                a.EndDate.HasValue &&
                a.EndDate.Value.ToUniversalTime() < now);

            // Tính tổng số người tham gia từ ActivityParticipants
            var totalParticipants = await _context.ActivityParticipants
                .Where(p => !p.IsDeleted)
                .CountAsync();

            return new ActivityStatisticsDto
            {
                OngoingCount = ongoingCount,
                UpcomingCount = upcomingCount,
                CompletedCount = completedCount,
                TotalParticipants = totalParticipants
            };
        }

        public async Task<ImportActivityResponseDto> ImportActivitiesAsync(IFormFile file)
        {
            var response = new ImportActivityResponseDto
            {
                Valid = true,
                Errors = new List<ImportErrorDto>(),
                Activities = new List<CreateActivityDto>()
            };

            // Validate file type
            var allowedExtensions = new[] { ".csv", ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                response.Valid = false;
                response.Errors.Add(new ImportErrorDto
                {
                    Row = 0,
                    Message = "Chỉ chấp nhận file CSV hoặc Excel (.csv, .xlsx, .xls)"
                });
                return response;
            }

            // Parse file
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var activities = new List<CreateActivityDto>();
            var errors = new List<ImportErrorDto>();
            int rowNumber = 0;

            try
            {
                if (fileExtension == ".csv")
                {
                    // Parse CSV
                    using var reader = new StreamReader(stream);
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        TrimOptions = TrimOptions.Trim,
                        MissingFieldFound = null
                    });

                    await csv.ReadAsync();
                    csv.ReadHeader();

                    while (await csv.ReadAsync())
                    {
                        rowNumber++;
                        var activity = ParseActivityRow(csv, rowNumber, errors);
                        if (activity != null)
                        {
                            activities.Add(activity);
                        }
                    }
                }
                else
                {
                    // Parse Excel
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header

                    foreach (var row in rows)
                    {
                        rowNumber++;
                        var activity = ParseActivityRowExcel(row, rowNumber, errors);
                        if (activity != null)
                        {
                            activities.Add(activity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors.Add(new ImportErrorDto
                {
                    Row = 0,
                    Message = $"Lỗi khi đọc file: {ex.Message}"
                });
                return response;
            }

            response.TotalRows = rowNumber;
            response.ValidRows = activities.Count;
            response.Errors = errors;
            response.Activities = activities;
            response.Valid = errors.Count == 0;

            return response;
        }

        private CreateActivityDto? ParseActivityRow(CsvReader csv, int rowNumber, List<ImportErrorDto> errors)
        {
            try
            {
                var title = csv.GetField<string>("Tên hoạt động")?.Trim();
                var subType = csv.GetField<string>("Loại hoạt động")?.Trim();
                var startDateStr = csv.GetField<string>("Ngày bắt đầu")?.Trim();
                var startTimeStr = csv.GetField<string>("Giờ bắt đầu")?.Trim();
                var endDateStr = csv.GetField<string>("Ngày kết thúc")?.Trim();
                var endTimeStr = csv.GetField<string>("Giờ kết thúc")?.Trim();
                var location = csv.GetField<string>("Địa điểm")?.Trim();
                var organizer = csv.GetField<string>("Đơn vị tổ chức")?.Trim();
                var description = csv.GetField<string>("Mô tả")?.Trim();
                var maxParticipantsStr = csv.GetField<string>("Số người tham gia tối đa")?.Trim();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(title))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Tên hoạt động", Message = "Tên hoạt động không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(subType))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Loại hoạt động", Message = "Loại hoạt động không được để trống" });
                    return null;
                }

                if (!new[] { "SeminarWorkshop", "CreativeContest", "SportsFestival" }.Contains(subType))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Loại hoạt động", Message = $"Loại hoạt động không hợp lệ. Chỉ chấp nhận: SeminarWorkshop, CreativeContest, SportsFestival" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(startDateStr))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày bắt đầu", Message = "Ngày bắt đầu không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(endDateStr))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = "Ngày kết thúc không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(location))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Địa điểm", Message = "Địa điểm không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(organizer))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Đơn vị tổ chức", Message = "Đơn vị tổ chức không được để trống" });
                    return null;
                }

                // Parse dates
                if (!DateTime.TryParse(startDateStr, out var startDate))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày bắt đầu", Message = $"Định dạng ngày không hợp lệ: {startDateStr}" });
                    return null;
                }

                if (!DateTime.TryParse(endDateStr, out var endDate))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = $"Định dạng ngày không hợp lệ: {endDateStr}" });
                    return null;
                }

                // Parse times if provided
                if (!string.IsNullOrWhiteSpace(startTimeStr))
                {
                    if (TimeSpan.TryParse(startTimeStr, out var startTime))
                    {
                        startDate = startDate.Date.Add(startTime);
                    }
                }

                if (!string.IsNullOrWhiteSpace(endTimeStr))
                {
                    if (TimeSpan.TryParse(endTimeStr, out var endTime))
                    {
                        endDate = endDate.Date.Add(endTime);
                    }
                }

                // Validate date logic
                if (endDate < startDate)
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = "Ngày kết thúc phải sau ngày bắt đầu" });
                    return null;
                }

                // Parse MaxParticipants
                int? maxParticipants = null;
                if (!string.IsNullOrWhiteSpace(maxParticipantsStr))
                {
                    if (int.TryParse(maxParticipantsStr, out var max))
                    {
                        if (max > 0)
                        {
                            maxParticipants = max;
                        }
                    }
                }

                // Determine Category from SubType
                var category = subType switch
                {
                    "SeminarWorkshop" => ActivityType.Activity,
                    "CreativeContest" => ActivityType.Activity,
                    "SportsFestival" => ActivityType.Event,
                    _ => ActivityType.Activity
                };

                var activity = new CreateActivityDto
                {
                    Title = title,
                    SubType = subType,
                    Category = category,
                    Description = description ?? $"{title} - {subType}",
                    StartDate = startDate,
                    EndDate = endDate,
                    Location = location,
                    Organizer = organizer,
                    MaxParticipants = maxParticipants,
                    RegisterDate = startDate.AddDays(-7), // Default: 7 days before start
                    EndRegisterDate = startDate.AddDays(-1), // Default: 1 day before start
                    ThumbnailUrl = "https://via.placeholder.com/400x300?text=Activity", // Default thumbnail
                    Rules = new List<string>(),
                    OnlyTeacherCanRegister = false
                };

                return activity;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto { Row = rowNumber, Message = $"Lỗi khi parse dòng: {ex.Message}" });
                return null;
            }
        }

        private CreateActivityDto? ParseActivityRowExcel(IXLRow row, int rowNumber, List<ImportErrorDto> errors)
        {
            try
            {
                var title = row.Cell(1).GetString()?.Trim();
                var subType = row.Cell(2).GetString()?.Trim();
                var startDateStr = row.Cell(3).GetString()?.Trim();
                var startTimeStr = row.Cell(4).GetString()?.Trim();
                var endDateStr = row.Cell(5).GetString()?.Trim();
                var endTimeStr = row.Cell(6).GetString()?.Trim();
                var location = row.Cell(7).GetString()?.Trim();
                var organizer = row.Cell(8).GetString()?.Trim();
                var description = row.Cell(9).GetString()?.Trim();
                var maxParticipantsStr = row.Cell(10).GetString()?.Trim();

                // Same validation logic as CSV
                if (string.IsNullOrWhiteSpace(title))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Tên hoạt động", Message = "Tên hoạt động không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(subType))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Loại hoạt động", Message = "Loại hoạt động không được để trống" });
                    return null;
                }

                if (!new[] { "SeminarWorkshop", "CreativeContest", "SportsFestival" }.Contains(subType))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Loại hoạt động", Message = $"Loại hoạt động không hợp lệ. Chỉ chấp nhận: SeminarWorkshop, CreativeContest, SportsFestival" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(startDateStr))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày bắt đầu", Message = "Ngày bắt đầu không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(endDateStr))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = "Ngày kết thúc không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(location))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Địa điểm", Message = "Địa điểm không được để trống" });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(organizer))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Đơn vị tổ chức", Message = "Đơn vị tổ chức không được để trống" });
                    return null;
                }

                // Parse dates (Excel might return DateTime directly)
                DateTime startDate;
                if (row.Cell(3).DataType == XLDataType.DateTime)
                {
                    startDate = row.Cell(3).GetDateTime();
                }
                else if (!DateTime.TryParse(startDateStr, out startDate))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày bắt đầu", Message = $"Định dạng ngày không hợp lệ: {startDateStr}" });
                    return null;
                }

                DateTime endDate;
                if (row.Cell(5).DataType == XLDataType.DateTime)
                {
                    endDate = row.Cell(5).GetDateTime();
                }
                else if (!DateTime.TryParse(endDateStr, out endDate))
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = $"Định dạng ngày không hợp lệ: {endDateStr}" });
                    return null;
                }

                // Parse times if provided
                if (!string.IsNullOrWhiteSpace(startTimeStr))
                {
                    if (TimeSpan.TryParse(startTimeStr, out var startTime))
                    {
                        startDate = startDate.Date.Add(startTime);
                    }
                }

                if (!string.IsNullOrWhiteSpace(endTimeStr))
                {
                    if (TimeSpan.TryParse(endTimeStr, out var endTime))
                    {
                        endDate = endDate.Date.Add(endTime);
                    }
                }

                // Validate date logic
                if (endDate < startDate)
                {
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "Ngày kết thúc", Message = "Ngày kết thúc phải sau ngày bắt đầu" });
                    return null;
                }

                // Parse MaxParticipants
                int? maxParticipants = null;
                if (!string.IsNullOrWhiteSpace(maxParticipantsStr))
                {
                    if (int.TryParse(maxParticipantsStr, out var max))
                    {
                        if (max > 0)
                        {
                            maxParticipants = max;
                        }
                    }
                }

                // Determine Category from SubType
                var category = subType switch
                {
                    "SeminarWorkshop" => ActivityType.Activity,
                    "CreativeContest" => ActivityType.Activity,
                    "SportsFestival" => ActivityType.Event,
                    _ => ActivityType.Activity
                };

                var activity = new CreateActivityDto
                {
                    Title = title,
                    SubType = subType,
                    Category = category,
                    Description = description ?? $"{title} - {subType}",
                    StartDate = startDate,
                    EndDate = endDate,
                    Location = location,
                    Organizer = organizer,
                    MaxParticipants = maxParticipants,
                    RegisterDate = startDate.AddDays(-7),
                    EndRegisterDate = startDate.AddDays(-1),
                    ThumbnailUrl = "https://via.placeholder.com/400x300?text=Activity",
                    Rules = new List<string>(),
                    OnlyTeacherCanRegister = false
                };

                return activity;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto { Row = rowNumber, Message = $"Lỗi khi parse dòng: {ex.Message}" });
                return null;
            }
        }

        public async Task<List<ActivityResponseDto>> BulkCreateActivitiesAsync(BulkCreateActivitiesDto dto)
        {
            var createdActivities = new List<ActivityResponseDto>();

            foreach (var activityDto in dto.Activities)
            {
                try
                {
                    var createdActivity = await AddAsync(activityDto);
                    createdActivities.Add(createdActivity);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other activities
                    // In production, you might want to collect these errors
                    Console.WriteLine($"Error creating activity '{activityDto.Title}': {ex.Message}");
                }
            }

            return createdActivities;
        }

        public async Task<ActivityResponseDto> DuplicateAsync(int activityId)
        {
            var originalActivity = await GetByIdAsync(activityId);
            if (originalActivity == null)
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);

            // Get full activity with all related data
            var fullActivity = await _repo.GetByIdWithIncludesAsync(activityId);
            if (fullActivity == null)
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);

            // Create new activity DTO from original
            var duplicateDto = new CreateActivityDto
            {
                Title = $"{originalActivity.Title} (Bản sao)",
                Description = originalActivity.Description,
                Category = originalActivity.Category,
                SubType = originalActivity.SubType,
                Location = originalActivity.Location,
                Organizer = originalActivity.Organizer,
                ThumbnailUrl = originalActivity.ThumbnailUrl,
                StartDate = originalActivity.StartDate ?? DateTime.UtcNow,
                EndDate = originalActivity.EndDate ?? DateTime.UtcNow,
                RegisterDate = originalActivity.RegisterDate,
                EndRegisterDate = originalActivity.EndRegisterDate,
                MaxParticipants = originalActivity.MaxParticipants,
                OnlyTeacherCanRegister = originalActivity.OnlyTeacherCanRegister ?? false,
                CompetitionType = fullActivity.ActivityDetail?.CompetitionType,
                Theme = fullActivity.ActivityDetail?.Theme,
                Genre = fullActivity.ActivityDetail?.Genre,
                PaperSize = fullActivity.ActivityDetail?.PaperSize,
                DrawingMedium = fullActivity.ActivityDetail?.DrawingMedium,
                TimeLimit = fullActivity.ActivityDetail?.TimeLimit,
                SubmissionFormat = fullActivity.ActivityDetail?.SubmissionFormat,
                ProblemText = originalActivity.ProblemText,
                ProblemFileUrl = originalActivity.ProblemFileUrl,
                SubmissionDeadline = originalActivity.SubmissionDeadline,
                Rules = fullActivity.Rules?.Where(r => !r.IsDeleted).Select(r => r.RuleText).ToList() ?? new List<string>(),
                SportsCategories = fullActivity.Sports?.Where(s => !s.IsDeleted).Select(s => s.SportName).ToList() ?? new List<string>(),
                SportsConfigurations = fullActivity.Sports?.Where(s => !s.IsDeleted).Select(s => new ActivitySportConfigDto
                {
                    SportName = s.SportName,
                    MaxMembers = s.MaxMembers
                }).ToList() ?? new List<ActivitySportConfigDto>(),
                Speakers = fullActivity.Speakers?.Where(s => !s.IsDeleted).OrderBy(s => s.Order).Select(s => new ActivitySpeakerDto
                {
                    Name = s.Name,
                    Title = s.Title,
                    Bio = s.Bio,
                    ImageUrl = s.ImageUrl
                }).ToList() ?? new List<ActivitySpeakerDto>(),
                ProgramItems = fullActivity.Programs?.Where(p => !p.IsDeleted).OrderBy(p => p.Order).Select(p => new ActivityProgramDto
                {
                    Title = p.Title,
                    Time = p.Time,
                    Description = p.Description
                }).ToList() ?? new List<ActivityProgramDto>(),
            };

            // Copy grading settings
            if (originalActivity.IsGrade == true && !string.IsNullOrEmpty(originalActivity.GradingSettings))
            {
                try
                {
                    duplicateDto.GradingSettings = JsonSerializer.Deserialize<GradingSettingsDto>(originalActivity.GradingSettings, RegistrationSettingsJsonOptions);
                }
                catch
                {
                    // Ignore if deserialization fails
                }
            }

            // Copy registration settings
            if (!string.IsNullOrEmpty(originalActivity.RegistrationSettings))
            {
                try
                {
                    duplicateDto.RegistrationSettings = JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(originalActivity.RegistrationSettings, RegistrationSettingsJsonOptions);
                }
                catch
                {
                    // Ignore if deserialization fails
                }
            }

            // Copy star point rewards
            if (originalActivity.RegistrationReward != null)
            {
                duplicateDto.RegistrationReward = new ActivityRegistrationRewardDto
                {
                    StarPoints = originalActivity.RegistrationReward.StarPoints
                };
            }

            if (fullActivity.ActivityRewards != null && fullActivity.ActivityRewards.Any(r => !r.IsDeleted))
            {
                duplicateDto.Awards = fullActivity.ActivityRewards
                    .Where(r => !r.IsDeleted)
                    .Select(r => new ActivityAwardDto
                    {
                        Name = r.Rank ?? "Giải thưởng",
                        Points = r.StarPoints
                    })
                    .ToList();
            }

            // Create the duplicate activity
            return await AddAsync(duplicateDto);
        }
    }
}
