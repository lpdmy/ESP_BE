using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class ClubJoinRequestController : ControllerBase
    {
        private readonly IClubJoinRequestService _clubJoinRequestService;
        private readonly IHttpContextService _httpContextService;
        public ClubJoinRequestController(IClubJoinRequestService clubJoinRequestService, IHttpContextService httpContextService)
        {
            _clubJoinRequestService = clubJoinRequestService;
            _httpContextService = httpContextService;
        }
        [HttpPost(ApiEndpoints.ClubJoinRequest.JoinRequest)]
        public async Task<IActionResult> CreateJoinRequest(CreateClubJoinRequestDto dto )
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubJoinRequestService.CreateJoinRequest(dto, user);
            return Ok(new ResponseDto<ClubJoinRequestDto>(
                       response,
                       message: "Gửi yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpGet(ApiEndpoints.ClubJoinRequest.JoinRequest)]
        public async Task<IActionResult> GetAllClubJoinRequest([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var response = await _clubJoinRequestService.GetAllClubJoinRequestAsync(paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubJoinRequestDto>>(
                       response,
                       message: "Lấy danh sách yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpPut(ApiEndpoints.ClubJoinRequest.ApproveJoinRequest)]
        public async Task<IActionResult> ApproveJoinRequest(int id)
        {
            var response = await _clubJoinRequestService.ApproveJoinRequest(id);
            return Ok(new ResponseDto<ClubJoinRequestDto>(
                       response,
                       message: "Phê duyệt yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpPut(ApiEndpoints.ClubJoinRequest.RejectJoinRequest)]
        public async Task<IActionResult> RejectJoinRequest(int id)
        {
            var response = await _clubJoinRequestService.RejectJoinRequest(id);
            return Ok(new ResponseDto<bool>(
                       response,
                       message: "Từ chối yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpPost(ApiEndpoints.ClubJoinRequest.InviteMentor)]
        public async Task<IActionResult> InviteMentor([FromBody] InviteMentorDto dto)
        {
            try
            {
                var response = await _clubJoinRequestService.InviteMentor(dto);
                return Ok(new ResponseDto<ClubJoinRequestDto>(
                    response,
                    message: "Mời giảng viên làm cố vấn câu lạc bộ thành công",
                    statusCode: 200
                ));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    message: ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi không xác định
                return StatusCode(500, new ResponseDto<string>(
                    null,
                    message: "Đã xảy ra lỗi trong quá trình xử lý",
                    statusCode: 500
                ));
            }
        }
        [HttpPut(ApiEndpoints.ClubJoinRequest.InviteMentor)]
        public async Task<IActionResult> MentorApprove(int id)
        {
            try {
                var response = await _clubJoinRequestService.MentorApprove(id);
                return Ok(new ResponseDto<ClubJoinRequestDto>(
                           response,
                           message: "Giảng viên đã chấp nhận làm cố vấn câu lạc bộ thành công",
                           statusCode: 200
                       ));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    message: ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(
                    null,
                    message: "Đã xảy ra lỗi trong quá trình xử lý",
                    statusCode: 500
                ));
            }
        }
        [HttpGet(ApiEndpoints.ClubJoinRequest.JoinRequestByClub)]
        public async Task<IActionResult> GetAllClubJoinRequestByCludId(int id, [FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var response = await _clubJoinRequestService.GetAllClubJoinRequestByCludId(id, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubJoinRequestDto>>(
                       response,
                       message: "Lấy danh sách yêu cầu tham gia câu lạc bộ theo ID câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpDelete(ApiEndpoints.ClubJoinRequest.JoinRequestId)]
        public async Task<IActionResult> CancelJoinRequest(int id)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubJoinRequestService.CancelJoinRequest(id, user);
            return Ok(new ResponseDto<ClubJoinRequestDto>(
                       response,
                       message: "Hủy yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpGet(ApiEndpoints.ClubJoinRequest.JoinRequestByUser)]
        public async Task<IActionResult> GetAllClubJoinRequestByUser([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubJoinRequestService.GetAllClubJoinRequestByUser(user.Id, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubJoinRequestDto>>(
                       response,
                       message: "Lấy danh sách yêu cầu tham gia câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }


    }
}
