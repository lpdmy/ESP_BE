using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.CustomUI;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduShpere.Application.Services
{
    public class JuryService : IJuryService
    {
        private readonly IJuryActivityRepository _juryActivityRepo;
        private readonly IJuryAssignRepository _juryAssignRepo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepo;
        private readonly INotificationService _notificationService;
        private readonly ISubmissionReposiory _submissionRepository;
        public JuryService(IJuryActivityRepository juryActivityRepo, IMapper mapper, IActivityRepository activityRepository, IJuryAssignRepository juryAssignRepo, INotificationService notificationService, ISubmissionReposiory submissionRepository)
        {
            _juryActivityRepo = juryActivityRepo;
            _mapper = mapper;
            _activityRepo = activityRepository;
            _juryAssignRepo = juryAssignRepo;
            _notificationService = notificationService;
            _submissionRepository = submissionRepository;
        }

        public async Task<List<JuryActivityResponseDto>> GetAllJuryByActivityIdAsync(int activityId, string? search)
        {
            var query = _juryActivityRepo.GetAllJuryActivityByActivityIdIncluding(activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.User.FirstName.Contains(search) ||
                    x.User.LastName.Contains(search));
            }

            // Load jury activities first
            var juryActivities = await query
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.ActivityId,
                    x.User.FirstName,
                    x.User.LastName
                })
                .ToListAsync();

            // Get all user IDs
            var userIds = juryActivities.Select(x => x.UserId).Distinct().ToList();

            // Load all assignment counts in one query using GroupBy
            var assignmentCounts = await _juryAssignRepo.GetQueryable()
                .Where(ja => userIds.Contains(ja.UserId))
                .GroupBy(ja => ja.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // Map results with assignment counts
            var result = juryActivities.Select(x => new JuryActivityResponseDto
            {
                Id = x.Id,
                UserId = x.UserId,
                ActivityId = x.ActivityId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Assigned = assignmentCounts.GetValueOrDefault(x.UserId, 0)
            }).ToList();

            return result;
        }

        public async Task<List<JuryActivityResponseDto>> CreateJury(CreateJuryDto dto)
        {
            // Only load what we need to validate and send notifications
            var activity = await _activityRepo.GetByIdAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var existingUserIds = await _juryActivityRepo.GetQueryable()
                .Where(ja => ja.ActivityId == dto.ActivityId)
                .Select(ja => ja.UserId)
                .ToListAsync();

            var juryToCreate = dto.JuryId
                .Except(existingUserIds)
                .Distinct()
                .ToList();

            if (!juryToCreate.Any())
            {
                return new List<JuryActivityResponseDto>();
            }

            var newJuryActivities = juryToCreate
                .Select(userId => new JuryActivity
                {
                    UserId = userId,
                    ActivityId = dto.ActivityId
                })
                .ToList();

            await _juryActivityRepo.AddRangeAsync(newJuryActivities);

            foreach (var userId in juryToCreate)
            {
                await _notificationService.AddAsync(new Notification
                {
                    Link= "/jury/dashboard",
                    Title = $" Bạn đã được thêm vào hội đồng chấm thi cuộc thi {activity.Title}",
                    CreatedAt = DateTime.Now,
                    Read = false,
                    Type = "system",
                    UserId = userId,
                });
            }

            return newJuryActivities
                .Select(j => new JuryActivityResponseDto
                {
                    UserId = j.UserId,
                    ActivityId = j.ActivityId,
                })
                .ToList();
        }
        public async Task DeletedJury(int id)
        {
            var juryActivity = await _juryActivityRepo.GetByIdAsync(id);
            if (juryActivity == null)
            {
                throw new BadRequestException(ErrorMessages.Jury.JuryNotFound);
            }
            await _juryActivityRepo.DeleteAsync(juryActivity.Id);
        }
        public async Task<List<JuryAssignmentDto>> AssignJury(AssignJuryDto dto)
        {
            var result = new List<JuryAssignmentDto>();
            var oldAssignments = await _juryAssignRepo.GetAllBySubmissionId(dto.SubmissionId);
            foreach (var old in oldAssignments)
            {
                await _juryAssignRepo.DeleteAsync(old.Id);
            }
            foreach (var userId in dto.UserId)
            {

                var assignJury = new JuryAssignment
                {
                    UserId = userId,
                    SubmissionId = dto.SubmissionId
                };

                await _juryAssignRepo.AddAsync(assignJury);
                result.Add(new JuryAssignmentDto
                {
                    UserId = assignJury.UserId,
                    SubmissionId = dto.SubmissionId,
                });

            }

            return result;
        }
        public async Task<PaginationResponseDto<JuryActivityResponseDto>> GetAllByUserAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryActivityRepo.GetAllJuryActivityByUser(user.Id);

            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Activity.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<IEnumerable<JuryActivityResponseDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryActivityResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<bool> AutoAssignRandomAsync(int activityId, int juryPerSubmission)
        {
            // Materialize queries first to avoid multiple database hits
            var submissions = await _submissionRepository.GetAllSubmissionsByActivityId(activityId)
                .Select(s => new { s.Id })
                .ToListAsync();
            
            var juryActivities = await _juryActivityRepo.GetAllJuryActivityByActivityIdIncluding(activityId)
                .Select(j => j.UserId)
                .Distinct()
                .ToListAsync();
            
            if (juryActivities.Count < juryPerSubmission)
                throw new BadRequestException("Số lượng giám khảo không đủ.");
            
            // Only load necessary data: SubmissionId and UserId pairs
            var existingAssignments = await _juryAssignRepo.GetQueryable()
                .Where(ja => ja.Submission.ActivityId == activityId)
                .Select(ja => new { ja.SubmissionId, ja.UserId })
                .ToListAsync();
            
            // Group existing assignments by submission for faster lookup
            var assignmentsBySubmission = existingAssignments
                .GroupBy(a => a.SubmissionId)
                .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).ToHashSet());
            
            var random = new Random();
            var newAssignments = new List<JuryAssignment>();
            
            foreach (var submission in submissions)
            {
                // Get already assigned jury IDs for this submission
                var assignedIds = assignmentsBySubmission.GetValueOrDefault(submission.Id, new HashSet<int>());
                int need = juryPerSubmission - assignedIds.Count;
                
                if (need <= 0) continue;
                
                // Get available jury IDs (not yet assigned to this submission)
                var available = juryActivities
                    .Where(uid => !assignedIds.Contains(uid))
                    .ToList();
                
                if (available.Count == 0) continue;
                
                // Optimize random selection using Fisher-Yates shuffle approach
                // Shuffle and take first 'need' items
                var selected = new List<int>();
                var availableCopy = new List<int>(available);
                
                for (int i = 0; i < need && availableCopy.Count > 0; i++)
                {
                    int randomIndex = random.Next(availableCopy.Count);
                    selected.Add(availableCopy[randomIndex]);
                    availableCopy.RemoveAt(randomIndex);
                }
                
                foreach (var uid in selected)
                {
                    newAssignments.Add(new JuryAssignment
                    {
                        SubmissionId = submission.Id,
                        UserId = uid
                    });
                }
            }
            
            if (newAssignments.Count > 0)
            {
                await _juryAssignRepo.AddRangeAsync(newAssignments);
            }
            return true;
        }
        public async Task<bool> DeleteAssignJury(int activityId)
        {
            var activity = await _activityRepo.GetByIdAsync(activityId);
            if (activityId <= 0 || activity==null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            await _juryAssignRepo.DeleteAllByActivityIdAsync(activityId);
            return true;
        }
        public async Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserAsync(int userid,int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserId(activityId, userid);
            var totalCount = await query.CountAsync();
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Submission.Title.Contains(paginationRequest.Search));
            }
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            
            // QUAN TRỌNG: Không throw exception khi không có dữ liệu
            // Trả về empty list là trạng thái hợp lệ (user chưa được phân công hoặc đã chấm hết)
            if (!data.Any())
            {
                return new PaginationResponseDto<JuryAssignmentDto>
                {
                    Data = new List<JuryAssignmentDto>(),
                    TotalCount = totalCount,
                    PageNumber = paginationRequest.PageNumber,
                    PageSize = paginationRequest.PageSize
                };
            }
            
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data).ToList();

            // Precompute submission order/codes for this activity
            var orderedSubmissionIds = await _submissionRepository.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return $"SUB-{(idx + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return idx + 1;
            }

            foreach (var item in mapped)
            {
                item.ScoreDetail = JsonSerializer.Deserialize<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
                item.Criteria = JsonSerializer.Deserialize<List<string>>(item.GradeSetting ?? "[]");
                
                // Hide student identity for jurors; expose only code/order
                if (item.Submission != null)
                {
                    item.Submission.SubmissionCode = BuildSubmissionCode(item.SubmissionId);
                    item.Submission.OrderNumber = BuildOrderNumber(item.SubmissionId);
                    item.Submission.FirstName = string.Empty;
                    item.Submission.LastName = string.Empty;
                    item.Submission.UserId = 0;
                    item.Submission.Class = null;
                    item.Submission.Users = new List<int>();
                }
            }
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryAssignmentDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserNotGradeAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserIdNotGrading(activityId, userid);
            var totalCount = await query.CountAsync();
            
            // Debug logging
            Console.WriteLine($"[DEBUG] GetAllAssignByUserNotGradeAsync - UserId: {userid}, ActivityId: {activityId}, TotalCount: {totalCount}");
            
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Submission.Title.Contains(paginationRequest.Search));
            }
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] GetAllAssignByUserNotGradeAsync - Data count after pagination: {data.Count}");
            
            // QUAN TRỌNG: Không throw exception khi không có dữ liệu
            // Trả về empty list là trạng thái hợp lệ (user chưa được phân công hoặc đã chấm hết)
            if (!data.Any())
            {
                return new PaginationResponseDto<JuryAssignmentDto>
                {
                    Data = new List<JuryAssignmentDto>(),
                    TotalCount = totalCount,
                    PageNumber = paginationRequest.PageNumber,
                    PageSize = paginationRequest.PageSize
                };
            }
            
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data).ToList();

            var orderedSubmissionIds = await _submissionRepository.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return $"SUB-{(idx + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return idx + 1;
            }

            foreach (var item in mapped)
            {
                item.ScoreDetail = JsonSerializer.Deserialize<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
                item.Criteria = JsonSerializer.Deserialize<List<string>>(item.GradeSetting ?? "[]");
                
                if (item.Submission != null)
                {
                    item.Submission.SubmissionCode = BuildSubmissionCode(item.SubmissionId);
                    item.Submission.OrderNumber = BuildOrderNumber(item.SubmissionId);
                    item.Submission.FirstName = string.Empty;
                    item.Submission.LastName = string.Empty;
                    item.Submission.UserId = 0;
                    item.Submission.Class = null;
                    item.Submission.Users = new List<int>();
                }
            }
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryAssignmentDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserGradeAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserIdGrading(activityId, userid);
            var totalCount = await query.CountAsync();
            
            // Debug logging
            Console.WriteLine($"[DEBUG] GetAllAssignByUserGradeAsync - UserId: {userid}, ActivityId: {activityId}, TotalCount: {totalCount}");
            
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Submission.Title.Contains(paginationRequest.Search));
            }
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] GetAllAssignByUserGradeAsync - Data count after pagination: {data.Count}");
            
            // QUAN TRỌNG: Không throw exception khi không có dữ liệu
            // Trả về empty list là trạng thái hợp lệ (user chưa được phân công hoặc đã chấm hết)
            if (!data.Any())
            {
                return new PaginationResponseDto<JuryAssignmentDto>
                {
                    Data = new List<JuryAssignmentDto>(),
                    TotalCount = totalCount,
                    PageNumber = paginationRequest.PageNumber,
                    PageSize = paginationRequest.PageSize
                };
            }
            
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data).ToList();

            var orderedSubmissionIds = await _submissionRepository.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return $"SUB-{(idx + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return idx + 1;
            }

            foreach (var item in mapped)
            {
                item.ScoreDetail = JsonSerializer.Deserialize<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
                item.Criteria = JsonSerializer.Deserialize<List<string>>(item.GradeSetting ?? "[]");
                
                if (item.Submission != null)
                {
                    item.Submission.SubmissionCode = BuildSubmissionCode(item.SubmissionId);
                    item.Submission.OrderNumber = BuildOrderNumber(item.SubmissionId);
                    item.Submission.FirstName = string.Empty;
                    item.Submission.LastName = string.Empty;
                    item.Submission.UserId = 0;
                    item.Submission.Class = null;
                    item.Submission.Users = new List<int>();
                }
            }
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryAssignmentDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        public async Task<List<JuryAssignmentDto>> GetAllAssignByUserNotGradeAllAsync(int userid, int activityId, string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserIdNotGrading(activityId, userid);
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Submission.Title.Contains(search));
            }
            
            var data = await query.ToListAsync();
            
            if (!data.Any())
            {
                return new List<JuryAssignmentDto>();
            }
            
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data).ToList();

            var orderedSubmissionIds = await _submissionRepository.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return $"SUB-{(idx + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return idx + 1;
            }

            foreach (var item in mapped)
            {
                item.ScoreDetail = JsonSerializer.Deserialize<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
                item.Criteria = JsonSerializer.Deserialize<List<string>>(item.GradeSetting ?? "[]");
                
                if (item.Submission != null)
                {
                    item.Submission.SubmissionCode = BuildSubmissionCode(item.SubmissionId);
                    item.Submission.OrderNumber = BuildOrderNumber(item.SubmissionId);
                    item.Submission.FirstName = string.Empty;
                    item.Submission.LastName = string.Empty;
                    item.Submission.UserId = 0;
                    item.Submission.Class = null;
                    item.Submission.Users = new List<int>();
                }
            }
            
            return mapped;
        }

        public async Task<List<JuryAssignmentDto>> GetAllAssignByUserGradeAllAsync(int userid, int activityId, string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserIdGrading(activityId, userid);
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Submission.Title.Contains(search));
            }
            
            var data = await query.ToListAsync();
            
            if (!data.Any())
            {
                return new List<JuryAssignmentDto>();
            }
            
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data).ToList();

            var orderedSubmissionIds = await _submissionRepository.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return $"SUB-{(idx + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var idx = orderedSubmissionIds.IndexOf(submissionId);
                return idx + 1;
            }

            foreach (var item in mapped)
            {
                item.ScoreDetail = JsonSerializer.Deserialize<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
                item.Criteria = JsonSerializer.Deserialize<List<string>>(item.GradeSetting ?? "[]");
                
                if (item.Submission != null)
                {
                    item.Submission.SubmissionCode = BuildSubmissionCode(item.SubmissionId);
                    item.Submission.OrderNumber = BuildOrderNumber(item.SubmissionId);
                    item.Submission.FirstName = string.Empty;
                    item.Submission.LastName = string.Empty;
                    item.Submission.UserId = 0;
                    item.Submission.Class = null;
                    item.Submission.Users = new List<int>();
                }
            }
            
            return mapped;
        }

        public async Task<string> GradeSubmission(int assignmentId, int userId, Dictionary<string, int> scores,string? comment,int totalScore)
        {
            var assignment = await _juryAssignRepo.GetDetailById(assignmentId);
            if (assignment == null)
            {
                throw new BadRequestException(ErrorMessages.Assignment.AssignMentNotFound);
            }
            // Kiểm tra quyền: User hiện tại phải là người được phân công chấm bài này (assignment này thuộc về user)
            // Điều này đảm bảo mỗi judge chỉ có thể chấm assignment của chính họ
            if (assignment.UserId != userId)
            {
                throw new BadRequestException(ErrorMessages.Assignment.UnauthorizedGrading);
            }
            var gradingCriteria = JsonSerializer.Deserialize<List<string>>(assignment.Submission.Activity.GradingSettings ?? "[]");
            foreach (var key in scores.Keys)
            {
                if (!gradingCriteria.Contains(key))
                    throw new BadRequestException($"Tiêu chí không hợp lệ: {key}");
            }
            var scoreJson = JsonSerializer.Serialize(scores);
            var reuslt =  _juryAssignRepo.GradeSubmission(scoreJson, assignmentId, comment, totalScore);
            return scoreJson;
        }

        public async Task<bool> IsAssignedToGrade(int userId, int submissionId)
        {
            return await _juryAssignRepo.IsExisting(userId, submissionId);
        }
        private static Dictionary<string, float>? DeserializeScore(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return null;

            try
            {
                var unescaped = Regex.Unescape(rawJson);

                return JsonSerializer.Deserialize<Dictionary<string, float>>(unescaped);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ActivityWithoutJuryDto>> GetActivitiesWithoutJuryAsync()
        {
            var juryActivityQuery = _juryActivityRepo.GetQueryable()
                .Where(ja => !ja.IsDeleted);

            var submissionQuery = _submissionRepository.GetQueryable()
                .Where(s => !s.IsDeleted);

            var activities = await _activityRepo.GetQueryable()
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Where(a => !juryActivityQuery.Any(ja => ja.ActivityId == a.Id && !ja.IsDeleted))
                .Select(a => new ActivityWithoutJuryDto
                {
                    Id = a.Id,
                    Title = a.Title ?? string.Empty,
                    Description = a.Description,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    SubmissionDeadline = a.SubmissionDeadline,
                    SubmissionCount = submissionQuery.Count(s => s.ActivityId == a.Id && !s.IsDeleted),
                    HasSubmissions = submissionQuery.Any(s => s.ActivityId == a.Id && !s.IsDeleted),
                    CreatedAt = a.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return activities;
        }

        public async Task<bool> ImprovedRandomAssignAsync(int activityId, int juryPerSubmission)
        {
            // Rule A: Check if jurors have already graded - don't remove them
            // Rule B: Distribute evenly (prioritize jurors with fewer assignments)
            // Rule C: Don't assign duplicates

            var activity = await _activityRepo.GetByIdAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Get all submissions for this activity
            var submissions = await _submissionRepository.GetAllSubmissionsByActivityId(activityId)
                .Select(s => new { s.Id })
                .ToListAsync();

            if (submissions.Count == 0)
            {
                throw new BadRequestException("Sự kiện này chưa có bài nộp nào.");
            }

            // Get all available jurors for this activity
            var juryActivities = await _juryActivityRepo.GetAllJuryActivityByActivityIdIncluding(activityId)
                .Select(j => j.UserId)
                .Distinct()
                .ToListAsync();

            if (juryActivities.Count < juryPerSubmission)
            {
                throw new BadRequestException($"Số lượng giám khảo không đủ. Cần ít nhất {juryPerSubmission} giám khảo.");
            }

            // Get existing assignments with grading status and ids
            var existingAssignments = await _juryAssignRepo.GetQueryable()
                .Where(ja => ja.Submission.ActivityId == activityId)
                .Select(ja => new
                {
                    ja.Id,
                    ja.SubmissionId,
                    ja.UserId,
                    HasGraded = ja.TotalScore != null || !string.IsNullOrEmpty(ja.ScoreTemp)
                })
                .ToListAsync();

            // Rule B: Calculate workload for each juror (number of assignments across all submissions)
            var jurorWorkload = existingAssignments
                .GroupBy(a => a.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var jurorId in juryActivities)
            {
                jurorWorkload.TryAdd(jurorId, 0);
            }

            var random = new Random();
            var newAssignments = new List<JuryAssignment>();
            var assignmentsToRemove = new List<int>();

            foreach (var submission in submissions)
            {
                var graded = existingAssignments
                    .Where(a => a.SubmissionId == submission.Id && a.HasGraded)
                    .Select(a => a.UserId)
                    .ToList();

                var ungraded = existingAssignments
                    .Where(a => a.SubmissionId == submission.Id && !a.HasGraded)
                    .ToList();

                // Keep only the best ungraded assignments up to the required slots
                var remainingSlots = Math.Max(juryPerSubmission - graded.Count, 0);
                var ungradedToKeep = ungraded
                    .OrderBy(a => jurorWorkload.GetValueOrDefault(a.UserId, 0))
                    .ThenBy(_ => random.Next())
                    .Take(remainingSlots)
                    .ToList();

                var ungradedToRemove = ungraded.Except(ungradedToKeep).ToList();

                foreach (var remove in ungradedToRemove)
                {
                    assignmentsToRemove.Add(remove.Id);
                    if (jurorWorkload.ContainsKey(remove.UserId))
                    {
                        jurorWorkload[remove.UserId] = Math.Max(0, jurorWorkload[remove.UserId] - 1);
                    }
                }

                var keptJurors = new HashSet<int>(graded);
                keptJurors.UnionWith(ungradedToKeep.Select(a => a.UserId));

                var need = juryPerSubmission - keptJurors.Count;
                if (need <= 0)
                {
                    continue;
                }

                var availableJurors = juryActivities
                    .Where(uid => !keptJurors.Contains(uid))
                    .OrderBy(uid => jurorWorkload[uid])
                    .ThenBy(_ => random.Next())
                    .Take(need)
                    .ToList();

                foreach (var jurorId in availableJurors)
                {
                    newAssignments.Add(new JuryAssignment
                    {
                        SubmissionId = submission.Id,
                        UserId = jurorId
                    });
                    jurorWorkload[jurorId]++;
                }
            }

            foreach (var assignmentId in assignmentsToRemove)
            {
                await _juryAssignRepo.DeleteAsync(assignmentId);
            }

            if (newAssignments.Count > 0)
            {
                await _juryAssignRepo.AddRangeAsync(newAssignments);
            }

            return true;
        }
    }

}
