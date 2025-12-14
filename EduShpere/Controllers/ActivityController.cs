using EduShpere.Application;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Middlewares;
using EduShpere.Application.DTOs.CommonDto;
using System.Linq;
using EduShpere.Infrastructure;
using System.Security.Claims;
using EduShpere.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class ActivityController : BaseController
    {
        private readonly IActivityService _Service;
        private readonly IMapper _mapper;
        private readonly IActivityParticipantService _APservice;
        private readonly ITournamentScheduleService _tournamentScheduleService;
        private readonly EduShpereDbContext _dbContext;
        private readonly IHttpContextService _HttpContextService;

        public ActivityController(IActivityService Service, IMapper mapper, IActivityParticipantService APservice, IHttpContextService HttpContextService, ITournamentScheduleService tournamentScheduleService, EduShpereDbContext dbContext)
        {
            _Service = Service;
            _mapper = mapper;
            _APservice = APservice;
            _tournamentScheduleService = tournamentScheduleService;
            _dbContext = dbContext;
            _HttpContextService = HttpContextService;
        }
        [HttpGet(ApiEndpoints.Activity.Activities)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetAllActivitys(int pageNumber, int pageSize, string? search = null)
        {
            var (Activity, totalCount) = await _Service.GetAllAsync(pageNumber, pageSize, search);
            
            // Map each item individually to avoid AfterMap issues with collections
            var ActivityDtos = new List<ActivityResponseDto>();
            foreach (var activity in Activity)
            {
                var dto = _mapper.Map<ActivityResponseDto>(activity);
                dto.NumberOfParticipants = await _APservice.CountNumberParticipantInActivity(dto.Id);
                ActivityDtos.Add(dto);
            }

            var result = new PaginationResponseDto<ActivityResponseDto>
            {
                Data = ActivityDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(new ResponseDto<PaginationResponseDto<ActivityResponseDto>>(
                result,
                "Lấy danh sách thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpGet(ApiEndpoints.Activity.GetListItems)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetListItems(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 30,
            [FromQuery] string? search = null,
            [FromQuery] string? subType = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string? organizer = null,
            [FromQuery] int? minParticipants = null,
            [FromQuery] int? maxParticipants = null,
            [FromQuery] string? sortBy = "StartDate",
            [FromQuery] bool sortDescending = true)
        {
            var filter = new ActivityListFilterDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SubType = subType,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Organizer = organizer,
                MinParticipants = minParticipants,
                MaxParticipants = maxParticipants,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            // Get current user ID from token
            int? userId = null;
            var user = HttpContext.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    // Try alternative claim names
                    userIdClaim = user.FindFirst("Id")?.Value ??
                                  user.FindFirst("id")?.Value ??
                                  user.FindFirst("sub")?.Value ??
                                  user.FindFirst("nameid")?.Value;
                }
                
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var result = await _Service.GetListItemsWithFilterAsync(filter, userId);
            return Ok(new ResponseDto<PaginationResponseDto<ActivityListItemDto>>(
                result,
                "Lấy danh sách hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpGet(ApiEndpoints.Activity.MyActivities)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMyActivities(int pageNumber, int pageSize, string? search = null, string? status = null)
        {
            var userId = _HttpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ResponseDto<string>(
                    null,
                    "Không thể xác định người dùng",
                    (int)HttpStatusCode.Unauthorized
                ));
            }

            var (Activity, totalCount) = await _Service.GetActivitiesByUserIdAsync(userId.Value, pageNumber, pageSize, search, status);
            
            // Check if Activity is null
            if (Activity == null)
            {
                Activity = new List<Activity>();
            }
            
            // Map each item individually with participant info
            var ActivityDtos = new List<MyActivityResponseDto>();
            if (Activity != null)
            {
                foreach (var activity in Activity)
                {
                    if (activity == null) continue;
                    
                    var dto = _mapper.Map<MyActivityResponseDto>(activity);
                    dto.NumberOfParticipants = await _APservice.CountNumberParticipantInActivity(dto.Id);
                    
                    // Get participant info for current user
                    var participant = activity.ActivityParticipants?.FirstOrDefault(p => p.UserId == userId.Value && !p.IsDeleted);
                    if (participant != null)
                    {
                        dto.RegisteredAt = participant.CreatedAt;
                        dto.ParticipationStatus = participant.Status;
                    }
                    
                    // Get star points from registration reward
                    if (activity.RegistrationReward != null && !activity.RegistrationReward.IsDeleted)
                    {
                        dto.StarPoints = activity.RegistrationReward.StarPoints;
                    }
                    
                    ActivityDtos.Add(dto);
                }
            }

            var result = new PaginationResponseDto<MyActivityResponseDto>
            {
                Data = ActivityDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(new ResponseDto<PaginationResponseDto<MyActivityResponseDto>>(
                result,
                "Lấy danh sách hoạt động của bạn thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpGet(ApiEndpoints.Activity.RecentInputs)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetRecentInputs([FromQuery] int take = 5)
        {
            var userId = _HttpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ResponseDto<string>(
                    null,
                    "Không thể xác định người dùng",
                    (int)HttpStatusCode.Unauthorized
                ));
            }

            if (take < 1) take = 1;
            if (take > 20) take = 20; // giới hạn để tránh trả về quá nhiều

            var result = await _Service.GetRecentInputsAsync(userId.Value, take);
            return Ok(new ResponseDto<RecentActivityInputsDto>(
                result,
                "Lấy dữ liệu nhập gần đây thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpGet(ApiEndpoints.Activity.GetActivityById)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetActivitys(int id)
        {
            var Activity = await _Service.GetByIdAsync(id);
            if (Activity == null)
            {
                return NotFound(new ResponseDto<string>(
                    null,
                    ErrorMessages.Activity.ActivityNotFound,
                    (int)HttpStatusCode.NotFound
                ));
            }
            var ActivityDto = _mapper.Map<ActivityResponseDto>(Activity);
            return Ok(new ResponseDto<ActivityResponseDto>(
                ActivityDto,
                "Lấy hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        [HttpPost(ApiEndpoints.Activity.Activities)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult>CreateActivity([FromBody] CreateActivityDto dto)
        {
            var Activity = await _Service.AddAsync(dto);
            var ActivityDto = _mapper.Map<ActivityResponseDto>(Activity);
            return Ok(new ResponseDto<ActivityResponseDto>(
                ActivityDto,
                "Tạo hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        [HttpPut(ApiEndpoints.Activity.Activities)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> UpdateActivity([FromBody] UpdateActivityDto dto)
        {
            var Activity = await _Service.UpdateAsync(dto);
            var ActivityDto = _mapper.Map<ActivityResponseDto>(Activity);
            return Ok(new ResponseDto<ActivityResponseDto>(
                ActivityDto,
                "Cập nhật hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpPost(ApiEndpoints.Activity.GenerateTournamentSchedule)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GenerateTournamentSchedule(int id, [FromBody] GenerateTournamentScheduleRequestDto dto)
        {
            // Validate activity exists
            var activity = await _Service.GetByIdAsync(id);
            if (activity == null)
            {
                return NotFound(new ResponseDto<string>(
                    null,
                    ErrorMessages.Activity.ActivityNotFound,
                    (int)HttpStatusCode.NotFound
                ));
            }

            // Set ActivityId from route
            dto.ActivityId = id;

            // Generate schedule using AI service
            var response = await _tournamentScheduleService.GenerateScheduleAsync(dto, _dbContext);

            if (!response.Success)
            {
                return BadRequest(new ResponseDto<GenerateTournamentScheduleResponseDto>(
                    response,
                    response.Explanation,
                    (int)HttpStatusCode.BadRequest
                ));
            }

            return Ok(new ResponseDto<GenerateTournamentScheduleResponseDto>(
                response,
                "Tạo lịch thi đấu thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpPost(ApiEndpoints.Activity.ApplyTournamentSchedule)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> ApplyTournamentSchedule(int id, [FromBody] ApplyTournamentScheduleRequestDto dto)
        {
            var activity = await _Service.GetByIdAsync(id);
            if (activity == null)
            {
                return NotFound(new ResponseDto<string>(
                    null,
                    ErrorMessages.Activity.ActivityNotFound,
                    (int)HttpStatusCode.NotFound
                ));
            }

            var response = await _tournamentScheduleService.ApplyScheduleAsync(id, dto, _dbContext);
            return Ok(new ResponseDto<ApplyTournamentScheduleResponseDto>(
                response,
                "Áp dụng lịch thi đấu thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpPost(ApiEndpoints.Activity.Import)]
        [Authorize(Roles = "Teacher,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportActivities([FromForm] ImportActivityFileDto dto)
        {
            if (dto?.File == null || dto.File.Length == 0)
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    "File không được để trống",
                    (int)HttpStatusCode.BadRequest
                ));
            }

            try
            {
                var result = await _Service.ImportActivitiesAsync(dto.File);
                return Ok(new ResponseDto<ImportActivityResponseDto>(
                    result,
                    result.Valid ? "File đã được kiểm tra thành công" : "File có lỗi, vui lòng kiểm tra lại",
                    (int)HttpStatusCode.OK
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    $"Lỗi khi import file: {ex.Message}",
                    (int)HttpStatusCode.BadRequest
                ));
            }
        }

        [HttpPost(ApiEndpoints.Activity.BulkCreate)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> BulkCreateActivities([FromBody] BulkCreateActivitiesDto dto)
        {
            try
            {
                var result = await _Service.BulkCreateActivitiesAsync(dto);
                return Ok(new ResponseDto<List<ActivityResponseDto>>(
                    result,
                    $"Đã tạo thành công {result.Count} hoạt động",
                    (int)HttpStatusCode.OK
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    $"Lỗi khi tạo hoạt động: {ex.Message}",
                    (int)HttpStatusCode.BadRequest
                ));
            }
        }

        [HttpPost(ApiEndpoints.Activity.TrainScheduleModel)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TrainScheduleModel()
        {
            await _tournamentScheduleService.TrainModelAsync(_dbContext);
            return Ok(new ResponseDto<string>(
                null,
                "Train ML model thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpGet(ApiEndpoints.Activity.Statistics)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _Service.GetStatisticsAsync();
            return Ok(new ResponseDto<ActivityStatisticsDto>(
                statistics,
                "Lấy thống kê hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }

        [HttpPost("api/activity/{id}/duplicate")]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> Duplicate(int id)
        {
            var result = await _Service.DuplicateAsync(id);
            return Ok(new ResponseDto<ActivityResponseDto>(
                result,
                "Nhân bản hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
    }
}
