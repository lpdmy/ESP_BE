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
        
        public ActivityController(
            IActivityService Service, 
            IMapper mapper, 
            IActivityParticipantService APservice,
            ITournamentScheduleService tournamentScheduleService,
            EduShpereDbContext dbContext) 
        {
            _Service = Service;
            _mapper = mapper;
            _APservice = APservice;
            _tournamentScheduleService = tournamentScheduleService;
            _dbContext = dbContext;
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
    }
}
