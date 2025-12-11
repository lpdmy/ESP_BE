using Azure;
using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduShpere.Shared;

namespace EduShpere.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }
        [HttpGet(ApiEndpoints.Staff.Staffs)]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStaffById(int id)
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
        [HttpPut]
        [Route(ApiEndpoints.Staff.Staffs)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStaff([FromBody] UpdateStaffDto dto)
        {
            try
            {
                var result = await _staffService.UpdateStaff(dto);
                return Ok(new ResponseDto<UserResponseDto>(
                            result,
                            message: "Cập nhật thông tin staff thành công",
                            statusCode: 200
                        ));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
        [HttpDelete(ApiEndpoints.Staff.GetStaffById)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                await _staffService.DeleteSoft(id);
                return Ok(new ResponseDto<string>(
                            null,
                            message: "Xóa staff thành công",
                            statusCode: 200
                        ));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
        [HttpPut]
        [Route(ApiEndpoints.Staff.GetStaffById)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecoveryStaff(int id)
        {
            try
            {
                await _staffService.RecoveryStaff(id);
                return Ok(new ResponseDto<string>(
                            null,
                            message: "Phục hồi staff thành công",
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
