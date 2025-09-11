using EduShpere.Application;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs;
using System.Net;
using AutoMapper;

namespace EduShpere.Controllers
{
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _Service;
        private readonly IMapper _mapper;
        public ActivityController(IActivityService Service, IMapper mapper) {
            _Service = Service;
            _mapper = mapper;
        }
        [HttpPost(ApiEndpoints.Activity.GetAllActivitys)]
        public async Task<IActionResult> GetAllActivitys(int pageNumber, int pageSize, string? search = null)
        {
            var Activitys = await _Service.GetAllAsync(pageNumber, pageSize, search);
            var ActivityDtos = _mapper.Map<IEnumerable<ActivityResponseDto>>(Activitys);

            return Ok(new ResponseDto<IEnumerable<ActivityResponseDto>>(
                ActivityDtos,
                "Lấy danh sách thành công",
                (int)HttpStatusCode.OK
            ));
        }
        [HttpGet(ApiEndpoints.Activity.GetActivity)]
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
        [HttpPost(ApiEndpoints.Activity.CreateActivity)]
        public async Task<IActionResult>CreateActivity(CreateActivityDto dto)
        {
            var Activity = await _Service.AddAsync(dto);
            var ActivityDto = _mapper.Map<ActivityResponseDto>(Activity);
            return Ok(new ResponseDto<ActivityResponseDto>(
                ActivityDto,
                "Tạo hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
    }
}
