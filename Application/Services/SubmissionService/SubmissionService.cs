using System.Collections.Generic;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using System.Security;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduShpere.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionReposiory _repo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IJuryAssignRepository _juryAssign;
        private readonly IActivityParticipantRepository _activityParticipantRepository;
        private readonly EduShpereDbContext _context;
        private readonly IAuditService _auditService;
        
        public SubmissionService(
            ISubmissionReposiory repo, 
            IMapper mapper, 
            IActivityRepository activityRepository, 
            IJuryAssignRepository juryAssign,
            IActivityParticipantRepository activityParticipantRepository,
            EduShpereDbContext context,
            IAuditService auditService
            ,IAttachmentRepository attachmentRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _activityRepository = activityRepository;
            _attachmentRepository = attachmentRepository;
            _juryAssign = juryAssign;
            _activityParticipantRepository = activityParticipantRepository;
            _context = context;
            _auditService = auditService;
        }

        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityId(
    int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null, int? currentUserId = null)
        {
            // Check if user is Teacher (jury) - if yes, use anonymous query
            bool isAnonymous = false;
            if (currentUserId.HasValue)
        {
                var userRole = await _context.Users
                    .Where(u => u.Id == currentUserId.Value)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();
                isAnonymous = userRole == UserRole.Teacher;
            }

            // Use optimized query (no deep ClassGroupMembers navigation)
            var query = isAnonymous 
                ? _repo.GetAllSubmissionsByActivityIdAnonymous(activityId)
                : _repo.GetAllSubmissionsByActivityId(activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title != null && c.Title.Contains(search));
            }
            
            // Order by CreatedAt for consistent ordering (needed for submission codes)
            query = query.OrderBy(s => s.CreatedAt);
            
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            
            // Get all submission IDs for batch counting
            var submissionIds = data.Select(s => s.Id).ToList();
            
            // Get jury counts for all submissions in a single query (fixes N+1 problem)
            var juryCounts = await _context.JuryAssignment
                .Where(ja => submissionIds.Contains(ja.SubmissionId))
                .GroupBy(ja => ja.SubmissionId)
                .Select(g => new { SubmissionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SubmissionId, x => x.Count);
            
            // Optimized: Get only GradingSettings instead of loading full activity with all includes
            var gradingSettings = await _activityRepository.GetGradingSettingsAsync(activityId);
            if (gradingSettings == null)
            {
                // Verify activity exists
                var activityExists = await _activityRepository.GetByIdAsync(activityId);
                if (activityExists == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            }
            
            // Precompute submission codes once for the whole activity (deterministic ordering)
            var allSubmissionIdsOrdered = await _repo.GetAllSubmissionsByActivityIdAnonymous(activityId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToListAsync();

            string BuildSubmissionCode(int submissionId)
            {
                var indexInAll = allSubmissionIdsOrdered.IndexOf(submissionId);
                return $"SUB-{(indexInAll + 1):D3}";
            }

            int BuildOrderNumber(int submissionId)
            {
                var indexInAll = allSubmissionIdsOrdered.IndexOf(submissionId);
                return indexInAll + 1;
            }

            if (isAnonymous)
            {
                // Return anonymous DTO for Teachers (jurors) - hide user information
                var anonymousMapped = new List<SubmissionResponseDto>();
                
                foreach (var submission in data)
                {
                    var submissionCode = BuildSubmissionCode(submission.Id);
                    var orderNumber = BuildOrderNumber(submission.Id);
                    
                    // Map ScoreDetail from ScoreTemp JSON
                    var juryAssignments = submission.JuryAssignments.Select(ja => 
                    {
                        var dto = new JuryAssignmentDto
                        {
                            Id = ja.Id,
                            UserId = ja.UserId,
                            SubmissionId = ja.SubmissionId,
                            ScoreTemp = ja.ScoreTemp,
                            TotalScore = ja.TotalScore ?? 0,
                            Comment = ja.Comment
                        };
                        
                        // Parse ScoreDetail from ScoreTemp JSON
                        if (!string.IsNullOrEmpty(ja.ScoreTemp))
                        {
                            try
                            {
                                dto.ScoreDetail = JsonConvert.DeserializeObject<Dictionary<string, float>>(ja.ScoreTemp);
                            }
                            catch
                            {
                                dto.ScoreDetail = new Dictionary<string, float>();
                            }
                        }
                        
                        return dto;
                    }).ToList();
                    
                    anonymousMapped.Add(new SubmissionResponseDto
                    {
                        Id = submission.Id,
                        ActivityId = submission.ActivityId,
                        // Hide user information for anonymous mode
                        UserId = 0, // Hidden
                        FirstName = string.Empty, // Hidden
                        LastName = string.Empty, // Hidden
                        Class = null, // Hidden
                        Users = new List<int>(), // Hidden
                        Title = submission.Title ?? string.Empty,
                        SubmissionCode = submissionCode,
                        OrderNumber = orderNumber,
                        NumberJurys = juryCounts.TryGetValue(submission.Id, out var count) ? count : 0,
                        GradeSettings = gradingSettings,
                        CreatedAt = submission.CreatedAt,
                        Score = submission.Score ?? 0,
                        ActivityName = string.Empty, // Not needed for anonymous view
                        Status = string.Empty, // Not set in this context
                        Attachments = submission.Attachments
                            .Where(a => !a.IsDeleted)
                            .Select(a => new SubmissionAttachmentDto
                            {
                                Url = a.FileUrl ?? string.Empty,
                                FileName = a.FileName ?? string.Empty,
                                FileType = a.FileType ?? string.Empty
                            })
                            .ToList(),
                        JuryAssignments = juryAssignments
                    });
                }
                
                return new PaginationResponseDto<SubmissionResponseDto>
                {
                    Data = anonymousMapped,
                    TotalCount = totalCount,
                    PageNumber = paginationRequest.PageNumber,
                    PageSize = paginationRequest.PageSize
                };
            }
            else
            {
                // Return normal DTO for Students/Admins but also include code/order for consistent display
                var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
                
                foreach (var item in mapped)
                {
                    item.NumberJurys = juryCounts.TryGetValue(item.Id, out var count) ? count : 0;
                    item.GradeSettings = gradingSettings;
                    item.SubmissionCode = BuildSubmissionCode(item.Id);
                    item.OrderNumber = BuildOrderNumber(item.Id);
                }
                
                return new PaginationResponseDto<SubmissionResponseDto>
                {
                    Data = mapped,
                    TotalCount = totalCount,
                    PageNumber = paginationRequest.PageNumber,
                    PageSize = paginationRequest.PageSize
                };
            }
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserId(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivity(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            
            // Get all submission IDs for batch counting
            var submissionIds = data.Select(s => s.Id).ToList();
            
            // Get jury counts for all submissions in a single query (fixes N+1 problem)
            var juryCounts = await _context.JuryAssignment
                .Where(ja => submissionIds.Contains(ja.SubmissionId))
                .GroupBy(ja => ja.SubmissionId)
                .Select(g => new { SubmissionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SubmissionId, x => x.Count);
            
            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            
            // Set jury counts from the dictionary (no additional queries)
            foreach (var item in mapped)
            {
                item.NumberJurys = juryCounts.TryGetValue(item.Id, out var count) ? count : 0;
            }
            
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserIdNotGrading(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivityNotGrading(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            // Get all submission IDs for batch counting
            var submissionIds = data.Select(s => s.Id).ToList();
            
            // Get jury counts for all submissions in a single query (fixes N+1 problem)
            var juryCounts = await _context.JuryAssignment
                .Where(ja => submissionIds.Contains(ja.SubmissionId))
                .GroupBy(ja => ja.SubmissionId)
                .Select(g => new { SubmissionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SubmissionId, x => x.Count);

            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            
            // Set jury counts from the dictionary (no additional queries)
            foreach (var item in mapped)
            {
                item.NumberJurys = juryCounts.TryGetValue(item.Id, out var count) ? count : 0;
                item.GradeSettings = activity.GradingSettings;
            }

            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserIDGrading(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivityGrading(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            // Get all submission IDs for batch counting
            var submissionIds = data.Select(s => s.Id).ToList();
            
            // Get jury counts for all submissions in a single query (fixes N+1 problem)
            var juryCounts = await _context.JuryAssignment
                .Where(ja => submissionIds.Contains(ja.SubmissionId))
                .GroupBy(ja => ja.SubmissionId)
                .Select(g => new { SubmissionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SubmissionId, x => x.Count);

            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            
            // Set jury counts from the dictionary (no additional queries)
            foreach (var item in mapped)
            {
                item.NumberJurys = juryCounts.TryGetValue(item.Id, out var count) ? count : 0;
                item.GradeSettings = activity.GradingSettings;
            }

            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<List<SubmissionResponseDto>> GetRankByActivityId(int id)
        {
            var submissions = _repo.GetAllSubmissionsByActivityId(id);
            var requiredJuryDict = await _juryAssign.NumberJuryRequiredByActivity(id);

            var completedSubmissions = _repo.FilterCompletedSubmissions(await submissions.ToListAsync(), requiredJuryDict);

            if (!completedSubmissions.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            foreach (var s in completedSubmissions)
            {
                var scores = s.JuryAssignments?
                    .Where(j => j.TotalScore.HasValue)
                    .Select(j => j.TotalScore.Value);
                s.Score = scores != null && scores.Any()
                    ? scores.Average()
                    : 0;
            }
            var ranked = completedSubmissions
                .OrderByDescending(s => s.Score)
                .ToList();
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(ranked);
            return mapped;
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByUser(
    int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUser(userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }

            var submissionList = await query
                .Include(s => s.JuryAssignments)
                .ToListAsync();

            if (!submissionList.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            var activityIds = submissionList.Select(s => s.ActivityId).Distinct().ToList();

            var requiredJuryDict = new Dictionary<int, int>();
            foreach (var activityId in activityIds)
            {
                var dict = await _juryAssign.NumberJuryRequiredByActivity(activityId);
                foreach (var kvp in dict)
                {
                    requiredJuryDict[kvp.Key] = kvp.Value;
                }
            }

            var mapped = _mapper.Map<List<SubmissionResponseDto>>(submissionList);

            foreach (var dto in mapped)
            {
                var submission = submissionList.First(s => s.Id == dto.Id);
                var completedCount = submission.JuryAssignments.Count(j => j.TotalScore.HasValue);
                var requiredCount = requiredJuryDict.TryGetValue(submission.Id, out var rc) ? rc : 0;

                dto.Status = completedCount >= requiredCount ? "Đã chấm xong" : "Đang chấm";
            }

            var pagedData = mapped
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToList();

            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = pagedData,
                TotalCount = mapped.Count,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }



        private List<string> GradeCriteria(string json)
        {
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public async Task<SubmissionResponseDto> GetSubmissionById(int id)
        {
            var submission = await _repo.GetSubmissionById(id);
            if (submission == null)
            {
                throw new BadRequestException(ErrorMessages.Submission.SubmissionNotFound);
            }
            var submissionWithAttachments = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
               .Where(s => s.Id == id && !s.IsDeleted)
               .FirstOrDefaultAsync();
            var mapped = _mapper.Map<SubmissionResponseDto>(submissionWithAttachments);
            // Load attachments
           
            foreach (var item in mapped.JuryAssignments)
            {
                item.ScoreDetail = JsonConvert.DeserializeObject<Dictionary<string, float>>(item.ScoreTemp ?? "{}");
            }
            return mapped;
        }
       
        public async Task<SubmissionStatusDto> GetSubmissionStatusAsync(int activityId, int userId)
        {
            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            var now = DateTime.UtcNow;
            var status = new SubmissionStatusDto
            {
                SubmissionDeadline = activity.SubmissionDeadline
            };

            // Kiểm tra user đã đăng ký tham gia chưa
            var isRegistered = await _activityParticipantRepository.IsAlreadyRegisteredAsync(userId, activityId);
            if (!isRegistered)
            {
                status.CanSubmit = false;
                status.Message = "Bạn chưa đăng ký tham gia hoạt động này.";
                return status;
            }

            // Kiểm tra activity có yêu cầu nộp bài không
            var isSubmissionRequired = activity.SubmissionDeadline.HasValue;
            if (!isSubmissionRequired)
            {
                status.CanSubmit = false;
                status.Message = "Hoạt động này không yêu cầu nộp bài.";
                return status;
            }

            // Kiểm tra đã đến thời gian mở đề chưa (StartDate)
            if (!activity.StartDate.HasValue || now < activity.StartDate.Value)
            {
                status.CanSubmit = false;
                status.Message = "Chưa đến thời gian mở đề. Đề sẽ được mở vào " + 
                    activity.StartDate.Value.ToString("dd/MM/yyyy HH:mm") + ".";
                return status;
            }

            // Kiểm tra đã quá hạn nộp bài chưa
            if (now > activity.SubmissionDeadline.Value)
            {
                status.CanSubmit = false;
                status.Message = "Đã quá hạn nộp bài (" + 
                    activity.SubmissionDeadline.Value.ToString("dd/MM/yyyy HH:mm") + ").";
                return status;
            }

            // Kiểm tra đã nộp bài chưa
            var existingSubmission = await _context.Submissions
                .Where(s => s.ActivityId == activityId && s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingSubmission != null)
            {
                status.HasSubmission = true;
                status.SubmissionDate = existingSubmission.CreatedAt;
                status.SubmissionFileUrl = existingSubmission.FileUrl;
                status.SubmissionId = existingSubmission.Id;
                status.CanSubmit = false; // Đã nộp rồi, không cho nộp lại (có thể thay đổi logic nếu cho phép nộp lại)
                status.Message = "Bạn đã nộp bài vào " + 
                    existingSubmission.CreatedAt.ToString("dd/MM/yyyy HH:mm") + ".";
            }
            else
            {
                status.CanSubmit = true;
                status.Message = null;
            }

            return status;
        }

        //public async Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto, int userId)
        //{
        //    // Validate activity
        //    var activity = await _activityRepository.GetByIdWithIncludesAsync(dto.ActivityId);
        //    if (activity == null)
        //    {
        //        throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
        //    }

        //    // Kiểm tra user đã đăng ký chưa
        //    var isRegistered = await _activityParticipantRepository.IsAlreadyRegisteredAsync(userId, dto.ActivityId);
        //    if (!isRegistered)
        //    {
        //        throw new BadRequestException("Bạn chưa đăng ký tham gia hoạt động này.");
        //    }

        //    // Validate deadline
        //    var now = DateTime.UtcNow;
        //    if (!activity.SubmissionDeadline.HasValue)
        //    {
        //        throw new BadRequestException("Hoạt động này không yêu cầu nộp bài.");
        //    }

        //    if (!activity.StartDate.HasValue || now < activity.StartDate.Value)
        //    {
        //        throw new BadRequestException("Chưa đến thời gian mở đề.");
        //    }

        //    if (now > activity.SubmissionDeadline.Value)
        //    {
        //        throw new BadRequestException("Đã quá hạn nộp bài.");
        //    }

        //    // Kiểm tra đã nộp bài chưa (có thể cho phép nộp lại nếu cần)
        //    var existingSubmission = await _context.Submissions
        //        .Where(s => s.ActivityId == dto.ActivityId && s.UserId == userId && !s.IsDeleted)
        //        .FirstOrDefaultAsync();

        //    if (existingSubmission != null)
        //    {
        //        // Update existing submission
        //        existingSubmission.FileUrl = dto.FileUrl;
        //        existingSubmission.Title = dto.Title;
        //        existingSubmission.UpdatedAt = now;
        //        //_auditService.SetAuditFieldsForUpdate(existingSubmission);
        //        await _context.SaveChangesAsync();

        //        return _mapper.Map<SubmissionResponseDto>(existingSubmission);
        //    }
        //    else
        //    {
        //        // Create new submission
        //        var submission = new Submission
        //        {
        //            ActivityId = dto.ActivityId,
        //            UserId = userId,
        //            FileUrl = dto.FileUrl,
        //            Title = dto.Title,
        //            CreatedAt = now,
        //            IsDeleted = false
        //        };

        //        //_auditService.SetAuditFieldsForCreate(submission);
        //        await _context.Submissions.AddAsync(submission);
        //        await _context.SaveChangesAsync();

        //        return _mapper.Map<SubmissionResponseDto>(submission);
        //    }
        //}

        public async Task<SubmissionResponseDto?> GetMySubmissionAsync(int activityId, int userId)
        {
            var submission = await _context.Submissions
                .Where(s => s.ActivityId == activityId && s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (submission == null)
                return null;

            return _mapper.Map<SubmissionResponseDto>(submission);
        }

        public async Task<SubmissionResponseDto> CreateSubmission(CreateSubmissionDto dto, int userId)
        {
            // Validate activity exists
            var activity = await _activityRepository.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Create submission
            var submission = new Submission
            {
                ActivityId = dto.ActivityId,
                UserId = userId,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            };

            // Add attachments
            foreach (var attachmentDto in (dto.Attachments ?? new List<SubmissionAttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachmentDto.Url))
                {
                    submission.Attachments.Add(new Attachment
                    {
                        FileUrl = attachmentDto.Url,
                        FileName = !string.IsNullOrWhiteSpace(attachmentDto.FileName) ? attachmentDto.FileName : null,
                        FileType = attachmentDto.FileType,
                        Submission = submission,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                }
            }

            await _repo.AddAsync(submission);
            var submissionDto = _mapper.Map<SubmissionResponseDto>(submission);
            return submissionDto;
        }

       
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetMySubmissions(int userId, PaginationRequestDto paginationRequest, string? search = null)
        {
            // Get all submissions by user from all activities
            var allSubmissions = await _repo.GetAllAsync();
            var userSubmissions = allSubmissions
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                userSubmissions = userSubmissions
                    .Where(s => s.Title != null && s.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = userSubmissions.Count;

            // Apply pagination
            var paginatedSubmissions = userSubmissions
                .OrderByDescending(s => s.CreatedAt)
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToList();

            // Load attachments for each submission
            var submissionsWithAttachments = new List<Submission>();
            foreach (var submission in paginatedSubmissions)
            {
                var submissionWithAttachments = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
                    .Where(s => s.Id == submission.Id)
                    .FirstOrDefaultAsync();
                if (submissionWithAttachments != null)
                {
                    submissionsWithAttachments.Add(submissionWithAttachments);
                }
            }

            var mapped = _mapper.Map<IEnumerable<SubmissionResponseDto>>(submissionsWithAttachments);
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        public async Task<SubmissionResponseDto?> GetMySubmissionByActivityId(int activityId, int userId)
        {
            var query = _repo.GetAllSubmissionsByActivityId(activityId)
                .Where(s => s.UserId == userId && !s.IsDeleted);

            var submission = await query.FirstOrDefaultAsync();
            if (submission == null)
            {
                return null;
            }

            return _mapper.Map<SubmissionResponseDto>(submission);
        }

        public async Task<SubmissionResponseDto> UpdateSubmission(UpdateSubmissionDto dto, int userId)
        {
            var submission = await _repo.GetByIdAsync(dto.Id);
            if (submission == null || submission.IsDeleted)
            {
                throw new BadRequestException("Submission không tồn tại");
            }

            // Check permission - only owner can update
            if (submission.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật submission này");
            }

            // Update title
            submission.Title = dto.Title;
            submission.UpdatedAt = DateTime.UtcNow;
            submission.UpdatedBy = userId;

            // Delete old attachments
            await _attachmentRepository.DeleteAttachmentBySubmissionId(submission.Id);

            // Add new attachments
            foreach (var attachmentDto in (dto.Attachments ?? new List<SubmissionAttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachmentDto.Url))
                {
                    submission.Attachments.Add(new Attachment
                    {
                        FileUrl = attachmentDto.Url,
                        FileName = !string.IsNullOrWhiteSpace(attachmentDto.FileName) ? attachmentDto.FileName : null,
                        FileType = attachmentDto.FileType,
                        Submission = submission,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                }
            }

            await _repo.UpdateAsync(submission);

            // Reload with attachments
            var updatedSubmission = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
                .Where(s => s.Id == submission.Id)
                .FirstOrDefaultAsync();

            return _mapper.Map<SubmissionResponseDto>(updatedSubmission);
        }

        public async Task<bool> DeleteSubmission(int id, int userId)
        {
            var submission = await _repo.GetByIdAsync(id);
            if (submission == null || submission.IsDeleted)
            {
                return false;
            }

            // Check permission - only owner or admin can delete
            // Note: You may want to check if user is admin here
            if (submission.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa submission này");
            }

            // Soft delete
            submission.IsDeleted = true;
            submission.UpdatedAt = DateTime.UtcNow;
            submission.UpdatedBy = userId;

            await _repo.UpdateAsync(submission);
            return true;
        }
    }
}
