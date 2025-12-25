using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Application.Services.NotificationService;
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
        private readonly IPointHistoryService _pointHistoryService;
        private readonly INotificationService _notificationService;
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
            EduShpereDbContext context,
            IPointHistoryService pointHistoryService,
            INotificationService notificationService)
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
            _pointHistoryService = pointHistoryService;
            _notificationService = notificationService;
        }
        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search = null)
        {
            var (items, totalCount) = await _repo.GetAllWithPagingAsync(pageNumber, pageSize, search);
            return (items ?? Enumerable.Empty<Activity>(), totalCount);
        }

        /// <summary>
        /// Get paginated list of activities with optimized DTO (for list view)
        /// Only selects necessary fields and calculates NumberOfParticipants in query
        /// </summary>
        public async Task<PaginationResponseDto<ActivityListItemDto>> GetAllOptimizedAsync(int pageNumber, int pageSize, string? search = null)
        {
            // Base query - filter deleted items
            var query = _context.Activities.Where(a => !a.IsDeleted);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                query = query.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchLower))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Get activity IDs for the current page (for efficient loading of related data)
            // Sort by StartDate DESC to match user list (most recent activities first)
            var activityIds = await query
                .OrderByDescending(a => a.StartDate ?? DateTime.MinValue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => a.Id)
                .ToListAsync();

            if (!activityIds.Any())
            {
                return new PaginationResponseDto<ActivityListItemDto>
                {
                    Data = new List<ActivityListItemDto>(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            // Load participant counts in batch (avoid N+1)
            var participantCounts = await _context.ActivityParticipants
                .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                .GroupBy(p => p.ActivityId)
                .Select(g => new { ActivityId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ActivityId, x => x.Count);

            // Load sports data in batch (only for SportsFestival activities)
            var sportsData = await _context.ActivitySports
                .Where(s => activityIds.Contains(s.ActivityId) && !s.IsDeleted)
                .Select(s => new
                {
                    s.ActivityId,
                    Sport = new ActivitySportDto
                    {
                        Id = s.Id,
                        ActivityId = s.ActivityId,
                        SportName = s.SportName,
                        IsCustom = s.IsCustom,
                        MaxMembers = s.MaxMembers
                    }
                })
                .ToListAsync();

            var sportsByActivity = sportsData
                .GroupBy(s => s.ActivityId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Sport).ToList());

            // Load participants data in batch (only basic info for list view)
            var participantsData = await _context.ActivityParticipants
                .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                .Select(p => new ActivityParticipantDto
                {
                    Id = p.Id,
                    ActivityId = p.ActivityId,
                    UserId = p.UserId,
                    UserFullName = p.User != null ? 
                        (string.IsNullOrEmpty(p.User.FirstName) && string.IsNullOrEmpty(p.User.LastName) 
                            ? null 
                            : $"{p.User.FirstName ?? ""} {p.User.LastName ?? ""}".Trim()) 
                        : null,
                    GroupCode = p.GroupCode,
                    SportId = p.SportId
                })
                .ToListAsync();

            var participantsByActivity = participantsData
                .GroupBy(p => p.ActivityId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Select only necessary fields from Activities
            var activitiesData = await _context.Activities
                .Where(a => activityIds.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Description,
                    a.StartDate,
                    a.EndDate,
                    a.RegisterDate,
                    a.EndRegisterDate,
                    a.Location,
                    a.Organizer,
                    a.MaxParticipants,
                    a.Category,
                    a.SubType,
                    a.ThumbnailUrl,
                    a.OnlyTeacherCanRegister,
                    a.GradingSettings,
                    a.RegistrationSettings,
                    a.IsDeleted
                })
                .ToListAsync();

            // Map to DTO and deserialize JSON fields, maintain order by activityIds
            var activities = activitiesData
                .OrderBy(a => activityIds.IndexOf(a.Id))
                .Select(a => new ActivityListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                RegisterDate = a.RegisterDate,
                EndRegisterDate = a.EndRegisterDate,
                Location = a.Location,
                Organizer = a.Organizer ?? string.Empty,
                MaxParticipants = a.MaxParticipants,
                Category = a.Category,
                SubType = a.SubType,
                ThumbnailUrl = a.ThumbnailUrl ?? string.Empty,
                NumberOfParticipants = participantCounts.ContainsKey(a.Id) ? participantCounts[a.Id] : 0,
                OnlyTeacherCanRegister = a.OnlyTeacherCanRegister ?? false,
                GradingSettings = a.GradingSettings,
                RegistrationSettings = !string.IsNullOrEmpty(a.RegistrationSettings)
                    ? JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(a.RegistrationSettings, RegistrationSettingsJsonOptions)
                    : null,
                Sports = sportsByActivity.ContainsKey(a.Id) ? sportsByActivity[a.Id] : new List<ActivitySportDto>(),
                Participants = participantsByActivity.ContainsKey(a.Id) ? participantsByActivity[a.Id] : new List<ActivityParticipantDto>(),
                IsDeleted = a.IsDeleted
            }).ToList();

            return new PaginationResponseDto<ActivityListItemDto>
            {
                Data = activities,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// L?y danh s�ch activities t?i �u cho list view - ch? tr? v? c�c field c?n thi?t, kh�ng load navigation properties
        /// </summary>
        public async Task<IEnumerable<ActivityListItemDto>> GetListItemsAsync()
        {
            // Query t?i �u: ch? select c�c field c?n thi?t, kh�ng include navigation properties
            // L�u ?: Kh�ng th? deserialize JSON trong LINQ query, n�n load raw data tr�?c
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

            // Deserialize v� map sang DTO sau khi �? load t? DB (tr�nh memory leak warning)
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
                // T�nh status d?a tr�n ng�y th�ng (s? d?ng UTC �? �?m b?o consistency)
                Status = a.StartDate.HasValue && a.EndDate.HasValue
                    ? (DateTime.UtcNow < a.StartDate.Value.ToUniversalTime()
                        ? "Sắp diễn ra"
                        : DateTime.UtcNow >= a.StartDate.Value.ToUniversalTime() && DateTime.UtcNow <= a.EndDate.Value.ToUniversalTime()
                        ? "Đang diễn ra"
                        : "Đã kết thúc")
                    : "Đang cập nhật"
            }).ToList();

            // T�nh s? l�?ng participants cho m?i activity (batch query �? t?i �u)
            var activityIds = activities.Select(a => a.Id).ToList();
            var participantCounts = await _context.ActivityParticipants
                .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                .GroupBy(p => p.ActivityId)
                .Select(g => new { ActivityId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ActivityId, x => x.Count);

            // G�n s? l�?ng participants
            foreach (var activity in activities)
            {
                activity.NumberOfParticipants = participantCounts.GetValueOrDefault(activity.Id, 0);
            }

            return activities;
        }

        /// <summary>
        /// L?y danh s�ch activities v?i filtering, paging v� sorting t?i �u
        /// M?c �?nh s?p x?p theo StartDate DESC (ho?t �?ng g?n nh?t)
        /// </summary>
        public async Task<PaginationResponseDto<ActivityListItemDto>> GetListItemsWithFilterAsync(ActivityListFilterDto filter, int? userId = null)
        {
            // Validation ��?c x? l? b?i PaginationRequestDto attributes
            // Set defaults n?u c?n
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 30;
            if (filter.PageSize > 100) filter.PageSize = 100; // Limit max page size

            // Build base query v?i filtering
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
            // S? d?ng UTC time �? �?m b?o consistency
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
            // Convert to UTC �? �?m b?o consistency
            if (filter.DateFrom.HasValue)
            {
                var dateFromUtc = filter.DateFrom.Value.ToUniversalTime();
                query = query.Where(a => 
                    a.StartDate.HasValue && 
                    a.StartDate.Value.ToUniversalTime() >= dateFromUtc);
            }

            // Apply DateTo filter
            // Convert to UTC v� set to end of day �? include c? ng�y ��
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
            // SortBy v� SortDescending �? ��?c k? th?a t? PaginationRequestDto
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

            // Deserialize v� map sang DTO
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
                // T�nh status d?a tr�n ng�y th�ng (s? d?ng UTC �? �?m b?o consistency)
                Status = a.StartDate.HasValue && a.EndDate.HasValue
                    ? (DateTime.UtcNow < a.StartDate.Value.ToUniversalTime()
                        ? "Sắp diễn ra"
                        : DateTime.UtcNow >= a.StartDate.Value.ToUniversalTime() && DateTime.UtcNow <= a.EndDate.Value.ToUniversalTime()
                        ? "Đang diễn ra"
                        : "Đã kết thúc")
                    : "Đang cập nhật"
            }).ToList();
            
            // T�nh s? l�?ng participants cho m?i activity (batch query �? t?i �u)
            var activityIds = activities.Select(a => a.Id).ToList();
            if (activityIds.Any())
            {
                var participantCounts = await _context.ActivityParticipants
                    .Where(p => activityIds.Contains(p.ActivityId) && !p.IsDeleted)
                    .GroupBy(p => p.ActivityId)
                    .Select(g => new { ActivityId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ActivityId, x => x.Count);

                // Check if user is registered for each activity (if userId provided)
                // Query t?t c? participants c?a user trong c�c activities n�y
                var userRegisteredActivityIds = new HashSet<int>();
                if (userId.HasValue && userId.Value > 0 && activityIds.Any())
                {
                    // Query ��n gi?n v� r? r�ng: t?m t?t c? ActivityParticipants c?a user trong c�c activities n�y
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
                var submittedActivityIds = await _context.Submissions
                .Where(s => activityIds.Contains(s.ActivityId) && s.UserId == userId && !s.IsDeleted)
                .Select(s => s.ActivityId)
                .Distinct()
                .ToListAsync();
                var userSubmittedActivityIds = submittedActivityIds.ToHashSet();
                // G�n s? l�?ng participants v� IsRegistered
                foreach (var activity in activities)
                {
                    activity.NumberOfParticipants = participantCounts.GetValueOrDefault(activity.Id, 0);
                    activity.IsRegistered = userRegisteredActivityIds.Contains(activity.Id);
                    activity.HasSubmitted = userSubmittedActivityIds.Contains(activity.Id);
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

        /// <summary>
        /// Get lightweight activity info for schedule generation - chỉ lấy các trường cần thiết
        /// Tối ưu performance bằng cách chỉ query các trường cần thiết
        /// </summary>
        public async Task<ActivityScheduleInfoDto?> GetScheduleInfoByIdAsync(int id)
        {
            var activity = await _context.Activities
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new ActivityScheduleInfoDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Location = a.Location,
                    Sports = a.Sports
                        .Where(s => !s.IsDeleted)
                        .Select(s => new ActivitySportBasicDto
                        {
                            Id = s.Id,
                            SportName = s.SportName
                        })
                        .ToList(),
                    Participants = a.ActivityParticipants
                        .Where(p => !p.IsDeleted)
                        .Select(p => new ActivityParticipantBasicDto
                        {
                            ClassGroupId = p.ClassGroupId,
                            IsDeleted = p.IsDeleted
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return activity;
        }

        /// <summary>
        /// Get lightweight activity info for registration form - chỉ lấy các trường cần thiết
        /// Tối ưu performance bằng cách chỉ query các trường cần thiết
        /// </summary>
        public async Task<ActivityRegisterInfoDto?> GetRegisterInfoByIdAsync(int id)
        {
            var activity = await _context.Activities
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new ActivityRegisterInfoDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Location = a.Location,
                    MaxParticipants = a.MaxParticipants,
                    NumberOfParticipants = a.ActivityParticipants.Count(p => !p.IsDeleted),
                    Category = a.Category,
                    SubType = a.SubType,
                    RegisterDate = a.RegisterDate,
                    EndRegisterDate = a.EndRegisterDate,
                    RegistrationSettings = a.RegistrationSettings,
                    Sports = a.Sports
                        .Where(s => !s.IsDeleted)
                        .Select(s => new ActivitySportRegisterDto
                        {
                            Id = s.Id,
                            SportName = s.SportName,
                            MaxMembers = s.MaxMembers,
                            Description = null // ActivitySport entity không có Description property
                        })
                        .ToList(),
                    Participants = a.ActivityParticipants
                        .Where(p => !p.IsDeleted)
                        .Select(p => new ActivityParticipantRegisterDto
                        {
                            UserId = p.UserId,
                            IsDeleted = p.IsDeleted
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return activity;
        }

        /// <summary>
        /// Get lightweight activity info for view detail - chỉ lấy các trường cần thiết
        /// Tối ưu performance bằng cách chỉ query các trường cần thiết
        /// </summary>
        public async Task<ActivityViewInfoDto?> GetViewInfoByIdAsync(int id)
        {
            var now = DateTime.UtcNow;
            var startDate = await _context.Activities
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => a.StartDate)
                .FirstOrDefaultAsync();

            var isProblemVisible = startDate.HasValue && now >= startDate.Value;

            var activity = await _context.Activities
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new ActivityViewInfoDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Location = a.Location,
                    MaxParticipants = a.MaxParticipants,
                    NumberOfParticipants = a.ActivityParticipants.Count(p => !p.IsDeleted),
                    Category = a.Category,
                    SubType = a.SubType,
                    ThumbnailUrl = a.ThumbnailUrl,
                    RegisterDate = a.RegisterDate,
                    EndRegisterDate = a.EndRegisterDate,
                    SubmissionDeadline = a.SubmissionDeadline,
                    ProblemText = isProblemVisible ? a.ProblemText : null,
                    ProblemFileUrl = isProblemVisible ? a.ProblemFileUrl : null,
                    IsProblemVisible = isProblemVisible,
                    IsDeleted = a.IsDeleted,
                    Sports = a.Sports
                        .Where(s => !s.IsDeleted)
                        .Select(s => new ActivitySportViewDto
                        {
                            Id = s.Id,
                            SportName = s.SportName,
                            MaxMembers = s.MaxMembers
                        })
                        .ToList(),
                    // Không load participants trong GetViewInfo - sẽ load riêng khi cần với paging
                    Participants = new List<ActivityParticipantViewDto>(),
                    Rules = a.Rules
                        .Where(r => !r.IsDeleted)
                        .Select(r => r.RuleText ?? "")
                        .ToList(),
                    Speakers = a.Speakers
                        .Where(s => !s.IsDeleted)
                        .Select(s => new ActivitySpeakerViewDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Title = s.Title,
                            Bio = s.Bio,
                            ImageUrl = s.ImageUrl,
                            Order = s.Order
                        })
                        .OrderBy(s => s.Order)
                        .ToList(),
                    Programs = a.Programs
                        .Where(p => !p.IsDeleted)
                        .Select(p => new ActivityProgramViewDto
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            Time = p.Time,
                            Order = p.Order
                        })
                        .OrderBy(p => p.Order)
                        .ToList(),
                    ActivityDetail = a.ActivityDetail != null ? new ActivityDetailViewDto
                    {
                        Id = a.ActivityDetail.Id,
                        CompetitionType = a.ActivityDetail.CompetitionType,
                        Theme = a.ActivityDetail.Theme,
                        Genre = a.ActivityDetail.Genre,
                        PaperSize = a.ActivityDetail.PaperSize,
                        DrawingMedium = a.ActivityDetail.DrawingMedium,
                        TimeLimit = a.ActivityDetail.TimeLimit,
                        SubmissionFormat = a.ActivityDetail.SubmissionFormat
                    } : null
                })
                .FirstOrDefaultAsync();

            return activity;
        }

        public async Task<PaginationResponseDto<ActivityParticipantViewDto>> GetParticipantsAsync(int activityId, int pageNumber, int pageSize, int? classGroupId = null)
        {
            var query = _context.ActivityParticipants
                .Where(p => p.ActivityId == activityId && !p.IsDeleted);

            // Filter theo lớp nếu có (cho hội thao)
            if (classGroupId.HasValue)
            {
                query = query.Where(p => p.ClassGroupId == classGroupId.Value);
            }

            var totalCount = await query.CountAsync();

            var participants = await query
                .OrderBy(p => p.ClassGroup != null ? p.ClassGroup.Grade : 999)
                .ThenBy(p => p.ClassGroup != null ? p.ClassGroup.Name : "")
                .ThenBy(p => p.User != null ? p.User.FirstName : "")
                .ThenBy(p => p.User != null ? p.User.LastName : "")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ActivityParticipantViewDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    ClassGroupId = p.ClassGroupId,
                    SportId = p.SportId,
                    Status = p.Status != null ? p.Status.ToString() : null,
                    IsDeleted = p.IsDeleted,
                    UserFullName = p.User != null ? (p.User.FirstName + " " + p.User.LastName).Trim() : null,
                    UserAvatarUrl = p.User != null ? p.User.AvatarUrl : null,
                    ClassGroupName = p.ClassGroup != null ? (p.ClassGroup.Grade != null ? p.ClassGroup.Grade.ToString() : "") + (p.ClassGroup.Name ?? "") : null,
                    Grade = p.ClassGroup != null ? p.ClassGroup.Grade : null,
                    SportName = p.Sport != null ? p.Sport.SportName : null
                })
                .ToListAsync();

            return new PaginationResponseDto<ActivityParticipantViewDto>
            {
                Data = participants,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
                // TotalPages là computed property, tự động tính từ TotalCount và PageSize
            };
        }

        public async Task<ActivityResponseDto> AddAsync(CreateActivityDto dto)
        {
            // Validation
            // Cho ph�p ng�y b?t �?u b?ng ng�y k?t th�c
            if (dto.StartDate > dto.EndDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
            }
            // Date validation removed - handled by frontend
            // Frontend ensures: Now ≤ RegisterDate ≤ EndRegisterDate ≤ StartDate ≤ EndDate
            // Validate MaxParticipants: n?u c� gi� tr? th? ph?i > 0, null = kh�ng gi?i h?n (�p d?ng cho t?t c? lo?i activity)
            if (dto.MaxParticipants.HasValue && dto.MaxParticipants.Value <= 0)
            {
                throw new BadRequestException(ErrorMessages.Activity.MaxParticipantGreaterThanZero);
            }

            // Validation cho SubmissionDeadline (ch? �p d?ng cho Activity c� n?p b�i)
            if (dto.SubmissionDeadline.HasValue)
            {
                // Ki?m tra SubType c� ph?i l� CreativeContest ho?c c� submission kh�ng
                var hasSubmission = string.Equals(dto.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase) ||
                                   dto.SubType?.ToLower().Contains("submission") == true ||
                                   dto.SubType?.ToLower().Contains("contest") == true;
                
                if (hasSubmission)
                {
                    // SubmissionDeadline ph?i >= StartDate
                    if (dto.SubmissionDeadline.Value < dto.StartDate)
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineBeforeStartDate);
                    }
                    
                    // SubmissionDeadline ph?i <= EndDate
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
                // Normalize dates: Start dates → 00:00:00 UTC, End dates → 23:59:59 UTC
                StartDate = NormalizeDateToUTC(dto.StartDate, isEndDate: false),
                EndDate = NormalizeDateToUTC(dto.EndDate, isEndDate: true),
                Location = dto.Location,
                Category = dto.Category,
                SubType = dto.SubType,
                ThumbnailUrl = dto.ThumbnailUrl,
                    Organizer = dto.Organizer,
                // Normalize dates: Start dates → 00:00:00 UTC, End dates → 23:59:59 UTC
                // This ensures date-only values are stored correctly regardless of timezone
                RegisterDate = NormalizeDateToUTC(dto.RegisterDate, isEndDate: false),
                EndRegisterDate = NormalizeDateToUTC(dto.EndRegisterDate, isEndDate: true),
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
                    ? NormalizeDateToUTC(dto.SubmissionDeadline.Value, isEndDate: true)
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
                    "Ch?y 100m", "Ch?y 400m", "Nh?y cao", "Nh?y xa", 
                    "N�m b�ng", "B�ng ��", "B�ng chuy?n", "B�ng r?"
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

            // Helper function: Ki?m tra xem ng�y �? qua ch�a (ch? so s�nh ng�y, b? qua gi?)
            bool IsDatePassed(DateTime? date)
            {
                if (!date.HasValue) return false;
                var dateOnly = date.Value.Date;
                var nowOnly = now.Date;
                return dateOnly <= nowOnly;
            }

            // Ch? validate n?u ng�y ch�a qua
            // Cho ph�p ng�y b?t �?u b?ng ng�y k?t th�c
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                // Ch? validate n?u �t nh?t m?t trong hai ng�y ch�a qua
                if (!IsDatePassed(startDate) && !IsDatePassed(endDate))
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
                }
            }
            if (registerDate.HasValue && endRegisterDate.HasValue && registerDate >= endRegisterDate)
            {
                // Ch? validate n?u �t nh?t m?t trong hai ng�y ch�a qua
                if (!IsDatePassed(registerDate) && !IsDatePassed(endRegisterDate))
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDayRegister);
                }
            }
            if (endRegisterDate.HasValue && startDate.HasValue && endRegisterDate >= startDate)
            {
                // Ch? validate n?u �t nh?t m?t trong hai ng�y ch�a qua
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
                    existingActivity.StartDate = NormalizeDateToUTC(dto.StartDate.Value, isEndDate: false);
                }
                if (dto.EndDate.HasValue)
                {
                    existingActivity.EndDate = NormalizeDateToUTC(dto.EndDate.Value, isEndDate: true);
                }
                if (dto.Location != null) existingActivity.Location = dto.Location;
                if (dto.Category.HasValue) existingActivity.Category = dto.Category.Value;
                if (dto.SubType != null) existingActivity.SubType = dto.SubType;
                if (dto.ThumbnailUrl != null) existingActivity.ThumbnailUrl = dto.ThumbnailUrl;
                if (dto.Organizer != null) existingActivity.Organizer = dto.Organizer;
                // Update MaxParticipants: null = kh�ng gi?i h?n, c� gi� tr? = gi?i h?n s? ng�?i
                // C� th? c?p nh?t cho t?t c? c�c lo?i activity
                if (dto.MaxParticipants.HasValue)
                {
                    existingActivity.MaxParticipants = dto.MaxParticipants.Value;
                }
                else
                {
                    // N?u MaxParticipants l� null, set th�nh null (kh�ng gi?i h?n)
                    existingActivity.MaxParticipants = null;
                }
                if (dto.RegisterDate.HasValue)
                {
                    existingActivity.RegisterDate = NormalizeDateToUTC(dto.RegisterDate.Value, isEndDate: false);
                }
                if (dto.EndRegisterDate.HasValue)
                {
                    existingActivity.EndRegisterDate = NormalizeDateToUTC(dto.EndRegisterDate.Value, isEndDate: true);
                }
                if (dto.ClubId.HasValue) existingActivity.ClubId = dto.ClubId;
                
                // Validation cho SubmissionDeadline (ch? �p d?ng cho Activity c� n?p b�i)
                if (dto.SubmissionDeadline.HasValue)
                {
                    var finalStartDate = dto.StartDate ?? existingActivity.StartDate;
                    var finalEndDate = dto.EndDate ?? existingActivity.EndDate;
                    
                    // Ch? validate n?u c�c ng�y ch�a qua
                    if (finalStartDate.HasValue && dto.SubmissionDeadline.Value < finalStartDate.Value)
                    {
                        // Ch? validate n?u �t nh?t m?t trong hai ng�y ch�a qua
                        if (!IsDatePassed(finalStartDate) && !IsDatePassed(dto.SubmissionDeadline))
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineBeforeStartDate);
                        }
                    }
                    
                    if (finalEndDate.HasValue && dto.SubmissionDeadline.Value > finalEndDate.Value)
                    {
                        // Ch? validate n?u �t nh?t m?t trong hai ng�y ch�a qua
                        if (!IsDatePassed(finalEndDate) && !IsDatePassed(dto.SubmissionDeadline))
                    {
                        throw new BadRequestException(ErrorMessages.Activity.SubmissionDeadlineAfterEndDate);
                        }
                    }
                    
                    existingActivity.SubmissionDeadline = NormalizeDateToUTC(dto.SubmissionDeadline.Value, isEndDate: true);
                }
                else if (dto.SubmissionDeadline == null && dto.SubType != null)
                {
                    // N?u SubmissionDeadline ��?c set th�nh null (x�a), ch? x�a n?u kh�ng ph?i CreativeContest
                    var hasSubmission = string.Equals(dto.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase);
                    if (!hasSubmission)
                    {
                        existingActivity.SubmissionDeadline = null;
                    }
                }
                
                // Update ProblemText v� ProblemFileUrl
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
                    "Ch?y 100m", "Ch?y 400m", "Nh?y cao", "Nh?y xa", 
                    "N�m b�ng", "B�ng ��", "B�ng chuy?n", "B�ng r?"
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
            // L?y m?t s? b?n ghi g?n nh?t �? tr�ch xu?t gi� tr? g?i ?, tr�nh load to�n b?
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
                Contacts = new List<string>() // Hi?n ch�a l�u contact trong Activity, tr? v? danh s�ch tr?ng
            };
        }

        public async Task<ActivityStatisticsDto> GetStatisticsAsync()
        {
            var now = DateTime.UtcNow;

            // L?y t?t c? activities ch�a b? x�a
            // Tối ưu: Đếm trực tiếp trong database thay vì load tất cả vào memory
            // Chạy tuần tự để tránh lỗi DbContext threading (EF Core không cho phép nhiều operations đồng thời trên cùng context)
            var ongoingCount = await _context.Activities
                .Where(a => !a.IsDeleted
                    && a.StartDate.HasValue
                    && a.EndDate.HasValue
                    && a.StartDate.Value <= now
                    && a.EndDate.Value >= now)
                .CountAsync();

            var upcomingCount = await _context.Activities
                .Where(a => !a.IsDeleted
                    && a.StartDate.HasValue
                    && a.StartDate.Value > now)
                .CountAsync();

            var completedCount = await _context.Activities
                .Where(a => !a.IsDeleted
                    && a.EndDate.HasValue
                    && a.EndDate.Value < now)
                .CountAsync();

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

        public async Task<int> AwardParticipationPointsAsync(int activityId)
        {
            var activity = await _repo.GetByIdAsync(activityId);
            if (activity == null)
            {
                throw new NotFoundException($"Activity with ID {activityId} not found");
            }

            if (activity.HasAwardedParticipationPoints)
            {
                return 0;
            }

            if (activity.EndDate == null || activity.EndDate.Value > DateTime.UtcNow)
            {
                throw new BadRequestException("Activity has not ended yet. Cannot award participation points.");
            }

            var registrationReward = await _registrationRewardRepo.GetByActivityIdAsync(activityId);
            if (registrationReward == null || registrationReward.StarPoints <= 0)
            {
                activity.HasAwardedParticipationPoints = true;
                await _repo.UpdateAsync(activity);
                return 0;
            }

            var participants = await _context.ActivityParticipants
                .Where(p => p.ActivityId == activityId 
                    && !p.IsDeleted 
                    && p.Status == ParticipantStatus.Joined)
                .ToListAsync();

            if (!participants.Any())
            {
                // Không có participants, đánh dấu đã xử lý
                activity.HasAwardedParticipationPoints = true;
                await _repo.UpdateAsync(activity);
                return 0;
            }

            int awardedCount = 0;
            var errors = new List<string>();

            // Cộng điểm cho từng participant
            foreach (var participant in participants)
            {
                try
                {
                    var description = $"Điểm tham gia hoạt động: {activity.Title}";
                    await _pointHistoryService.AddPointsWithTransactionAsync(
                        participant.UserId,
                        registrationReward.StarPoints,
                        description,
                        PointActionType.Earn
                    );
                    
                    // Gửi notification cho user
                    try
                    {
                        await _notificationService.AddAsync(new Notification
                        {
                            UserId = participant.UserId,
                            Title = $"Bạn đã nhận được {registrationReward.StarPoints} điểm tham gia từ hoạt động \"{activity.Title}\"",
                            Type = "starpoint",
                            CreatedAt = DateTime.UtcNow,
                            Read = false,
                            Link = $"/activities/{activityId}"
                        });
                    }
                    catch (Exception notifEx)
                    {
                        // Log notification error nhưng không fail việc cộng điểm
                        // Có thể log vào hệ thống logging nếu cần
                    }
                    
                    awardedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to award points to user {participant.UserId}: {ex.Message}");
                    // Continue with other participants
                }
            }

            if (errors.Any() && awardedCount == 0)
            {
                throw new Exception($"Failed to award points to all participants. Errors: {string.Join("; ", errors)}");
            }

            // QUAN TRỌNG: Đánh dấu đã cộng điểm sau khi thành công
            // Chỉ đánh dấu nếu có ít nhất 1 người được cộng điểm hoặc không có lỗi nghiêm trọng
            if (awardedCount > 0 || errors.Count == 0)
            {
                activity.HasAwardedParticipationPoints = true;
                await _repo.UpdateAsync(activity);
            }

            return awardedCount;
        }

        /// <summary>
        /// Trao điểm thưởng cho participants dựa trên rank (ActivityReward)
        /// participantRanks: Dictionary<participantId, rank> (ví dụ: "Giải Nhất", "Giải Nhì", "Giải Ba")
        /// </summary>
        public async Task<bool> AwardRankRewardsAsync(int activityId, Dictionary<int, string> participantRanks)
        {
            var activity = await _repo.GetByIdAsync(activityId);
            if (activity == null)
            {
                throw new NotFoundException($"Activity with ID {activityId} not found");
            }

            if (participantRanks == null || !participantRanks.Any())
            {
                throw new BadRequestException("Participant ranks cannot be empty");
            }

            // Lấy danh sách ActivityReward theo rank
            var rewards = await _awardRepo.GetByActivityIdAsync(activityId);
            if (rewards == null || !rewards.Any())
            {
                throw new BadRequestException($"No reward configuration found for activity {activityId}");
            }

            // Tạo dictionary để tra cứu điểm theo rank
            var rewardByRank = rewards.ToDictionary(r => r.Rank, r => r.StarPoints, StringComparer.OrdinalIgnoreCase);

            int awardedCount = 0;
            var errors = new List<string>();

            // Trao điểm cho từng participant
            foreach (var kvp in participantRanks)
            {
                var participantId = kvp.Key;
                var rank = kvp.Value;

                if (string.IsNullOrWhiteSpace(rank))
                {
                    errors.Add($"Participant {participantId} has empty rank");
                    continue;
                }

                // Kiểm tra rank có trong reward config không
                if (!rewardByRank.ContainsKey(rank))
                {
                    errors.Add($"Rank '{rank}' not found in reward configuration for participant {participantId}");
                    continue;
                }

                var points = rewardByRank[rank];
                if (points <= 0)
                {
                    errors.Add($"Rank '{rank}' has invalid points ({points}) for participant {participantId}");
                    continue;
                }

                // Kiểm tra participant có tồn tại không
                var participant = await _context.ActivityParticipants
                    .FirstOrDefaultAsync(p => p.Id == participantId && p.ActivityId == activityId && !p.IsDeleted);

                if (participant == null)
                {
                    errors.Add($"Participant {participantId} not found or deleted");
                    continue;
                }

                try
                {
                    var description = $"Giải thưởng {rank} - Hoạt động: {activity.Title}";
                    await _pointHistoryService.AddPointsWithTransactionAsync(
                        participant.UserId,
                        points,
                        description,
                        PointActionType.Earn
                    );
                    
                    // Gửi notification cho user
                    try
                    {
                        await _notificationService.AddAsync(new Notification
                        {
                            UserId = participant.UserId,
                            Title = $"Chúc mừng! Bạn đã nhận được {points} điểm từ giải thưởng \"{rank}\" - Hoạt động \"{activity.Title}\"",
                            Type = "starpoint",
                            CreatedAt = DateTime.UtcNow,
                            Read = false,
                            Link = $"/activities/{activityId}"
                        });
                    }
                    catch (Exception notifEx)
                    {
                        // Log notification error nhưng không fail việc cộng điểm
                        // Có thể log vào hệ thống logging nếu cần
                    }
                    
                    awardedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to award points to participant {participantId} (user {participant.UserId}): {ex.Message}");
                }
            }

            if (errors.Any() && awardedCount == 0)
            {
                throw new Exception($"Failed to award rank rewards. Errors: {string.Join("; ", errors)}");
            }

            return awardedCount > 0;
        }

        /// <summary>
        /// Tự động cộng điểm tham gia cho activity (dùng cho Power Automate)
        /// Trả về thông tin chi tiết về kết quả
        /// </summary>
        public async Task<AutoAwardPointsResponseDto> AutoAwardParticipationPointsAsync(int activityId)
        {
            var response = new AutoAwardPointsResponseDto
            {
                ActivityId = activityId,
                Success = false,
                AwardedCount = 0
            };

            try
            {
                var activity = await _repo.GetByIdAsync(activityId);
                if (activity == null)
                {
                    response.ErrorMessage = $"Activity with ID {activityId} not found";
                    return response;
                }

                response.ActivityTitle = activity.Title;

                // QUAN TRỌNG: Kiểm tra đã cộng điểm chưa
                if (activity.HasAwardedParticipationPoints)
                {
                    response.Success = true;
                    response.AwardedCount = 0;
                    response.Message = "Activity đã được cộng điểm trước đó";
                    return response;
                }

                // Kiểm tra activity đã kết thúc chưa
                if (activity.EndDate == null || activity.EndDate.Value.ToUniversalTime() > DateTime.UtcNow)
                {
                    response.ErrorMessage = "Activity has not ended yet. Cannot award participation points.";
                    return response;
                }

                // Gọi method hiện có để cộng điểm (method này sẽ tự động set flag HasAwardedParticipationPoints = true)
                var awardedCount = await AwardParticipationPointsAsync(activityId);

                response.Success = true;
                response.AwardedCount = awardedCount;
                response.Message = awardedCount > 0 
                    ? $"Đã cộng điểm tham gia cho {awardedCount} người tham gia thành công"
                    : "Không có người tham gia nào để cộng điểm hoặc không có cấu hình điểm thưởng";

                return response;
            }
            catch (BadRequestException ex)
            {
                // BadRequestException là lỗi hợp lệ (chưa kết thúc, đã cộng điểm, etc.)
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// Batch cộng điểm cho nhiều activities đã kết thúc (dùng cho Power Automate)
        /// </summary>
        public async Task<BatchAutoAwardPointsResponseDto> BatchAutoAwardParticipationPointsAsync(List<int> activityIds)
        {
            var response = new BatchAutoAwardPointsResponseDto
            {
                Success = true,
                TotalProcessed = 0,
                TotalAwarded = 0,
                TotalErrors = 0,
                Results = new List<AutoAwardPointsResponseDto>()
            };

            if (activityIds == null || !activityIds.Any())
            {
                response.Success = false;
                return response;
            }

            foreach (var activityId in activityIds)
            {
                var result = await AutoAwardParticipationPointsAsync(activityId);
                response.Results.Add(result);
                response.TotalProcessed++;

                if (result.Success && result.AwardedCount > 0)
                {
                    response.TotalAwarded += result.AwardedCount;
                }
                else if (!result.Success)
                {
                    response.TotalErrors++;
                }
            }

            response.Success = response.TotalErrors == 0;
            return response;
        }

        /// <summary>
        /// Tự động cộng điểm cho tất cả activities đã kết thúc nhưng chưa được cộng điểm (dùng cho Power Automate daily job)
        /// Optimized query để performance tốt - chỉ query các fields cần thiết
        /// </summary>
        public async Task<BatchAutoAwardPointsResponseDto> AutoAwardAllEndedActivitiesAsync()
        {
            var response = new BatchAutoAwardPointsResponseDto
            {
                Success = true,
                TotalProcessed = 0,
                TotalAwarded = 0,
                TotalErrors = 0,
                Results = new List<AutoAwardPointsResponseDto>()
            };

            var now = DateTime.UtcNow;

            // OPTIMIZED QUERY: Chỉ query các fields cần thiết và filter ngay trong database
            // Query activities đã kết thúc nhưng chưa được cộng điểm và có cấu hình điểm thưởng
            var activitiesToProcess = await _context.Activities
                .Where(a => !a.IsDeleted
                    && a.EndDate.HasValue
                    && a.EndDate.Value < now
                    && !a.HasAwardedParticipationPoints
                    && a.RegistrationReward != null
                    && a.RegistrationReward.StarPoints > 0
                    && !a.RegistrationReward.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.EndDate
                })
                .OrderBy(a => a.EndDate) // Xử lý activities cũ nhất trước
                .ToListAsync();

            if (!activitiesToProcess.Any())
            {
                response.Message = "Không có activity nào cần cộng điểm";
                return response;
            }

            // Process từng activity
            foreach (var activityInfo in activitiesToProcess)
            {
                try
                {
                    var result = await AutoAwardParticipationPointsAsync(activityInfo.Id);
                    response.Results.Add(result);
                    response.TotalProcessed++;

                    if (result.Success && result.AwardedCount > 0)
                    {
                        response.TotalAwarded += result.AwardedCount;
                    }
                    else if (!result.Success)
                    {
                        response.TotalErrors++;
                    }
                }
                catch (Exception ex)
                {
                    // Log error nhưng tiếp tục xử lý các activities khác
                    response.Results.Add(new AutoAwardPointsResponseDto
                    {
                        Success = false,
                        ActivityId = activityInfo.Id,
                        ActivityTitle = activityInfo.Title,
                        AwardedCount = 0,
                        ErrorMessage = ex.Message
                    });
                    response.TotalProcessed++;
                    response.TotalErrors++;
                }
            }

            response.Success = response.TotalErrors == 0;
            response.Message = $"Đã xử lý {response.TotalProcessed} activities: {response.TotalAwarded} người được cộng điểm, {response.TotalErrors} lỗi";

            return response;
        }

        /// <summary>
        /// Normalize date to UTC with default time:
        /// - Start dates: 00:00:00 UTC
        /// - End dates: 23:59:59 UTC
        /// This ensures date-only values are stored correctly regardless of timezone
        /// </summary>
        private static DateTime NormalizeDateToUTC(DateTime date, bool isEndDate = false)
        {
            // If date is already UTC, use it directly
            if (date.Kind == DateTimeKind.Utc)
            {
                if (isEndDate)
                {
                    // Set to end of day: 23:59:59.999
                    return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, DateTimeKind.Utc);
                }
                else
                {
                    // Set to start of day: 00:00:00.000
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, DateTimeKind.Utc);
                }
            }

            // Convert to UTC first
            var utcDate = date.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                : date.ToUniversalTime();

            // Normalize to start or end of day
            if (isEndDate)
            {
                // End date: 23:59:59.999 UTC
                return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 23, 59, 59, 999, DateTimeKind.Utc);
            }
            else
            {
                // Start date: 00:00:00.000 UTC
                return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            }
        }
    }
}
