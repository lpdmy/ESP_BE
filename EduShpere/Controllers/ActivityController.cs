using EduShpere.Application;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs;
using System.Net;
using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Middlewares;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class ActivityController : BaseController
    {
        private readonly IActivityService _Service;
        private readonly IMapper _mapper;
        private readonly IActivityParticipantService _APservice;
        public ActivityController(IActivityService Service, IMapper mapper, IActivityParticipantService APservice) {
            _Service = Service;
            _mapper = mapper;
            _APservice = APservice;
        }
        [HttpGet(ApiEndpoints.Activity.Activities)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetAllActivitys(int pageNumber, int pageSize, string? search = null)
        {
            var (Activity, totalCount) = await _Service.GetAllAsync(pageNumber, pageSize, search);
            var ActivityDtos = _mapper.Map<IEnumerable<ActivityResponseDto>>(Activity);

            foreach (var dto in ActivityDtos)
            {
                dto.numberOfParticipants = await _APservice.CountNumberParticipantInActivity(dto.Id);
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
    }
}
