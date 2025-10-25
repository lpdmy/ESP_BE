using Azure;
using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Shared;

namespace EduShpere.Controllers
{
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService) {
            _staffService = staffService;
        }
        [HttpGet(ApiEndpoints.Staff.Staffs)]
        public async Task<IActionResult> GetAllStaffs([FromQuery] PaginationRequestDto paginationRequest)
        {
            var result = await _staffService.GetAllAsync(paginationRequest);

            return Ok(new ResponseDto<PaginationResponseDto<UserResponseDto>>(
                        result,
                        message: "Lấy bộ sưu tập thành công",
                        statusCode: 200
                    ));
        }
        [HttpGet(ApiEndpoints.Staff.GetStaffById)]
        public async Task<IActionResult>GetStaffById (int id)
        {
            try
            {
                var result = await _staffService.GetByIdAsync(id);
                return Ok(new ResponseDto<UserResponseDto>(
                            result,
                            message: "Lấy thông tin staff thành công",
                            statusCode: 200
                        ));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            
        }

    }
}
