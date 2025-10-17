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
    public class ClubMemberController : ControllerBase
    {
        private readonly IClubMemberService _clubMemberService;
        private readonly IHttpContextService _httpContextService;
        public ClubMemberController(IClubMemberService clubMemberService, IHttpContextService httpContextService)
        {
            _clubMemberService = clubMemberService;
            _httpContextService = httpContextService;
        }
        [HttpGet(ApiEndpoints.ClubMember.GetClubMemberByUser)]
        public async Task<IActionResult> GetAllClubMember([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubMemberService.GetAllByUserAsync(user, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubMemberResponseDto>>(
                       response,
                       message: "Lấy danh sách thành viên câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpDelete(ApiEndpoints.ClubMember.OutClub)]
        public async Task<IActionResult> OutClub(int id)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var response = await _clubMemberService.OutClub(id, user);
                return Ok(new ResponseDto<ClubMemberResponseDto>(
                           response,
                           message: "Rời câu lạc bộ thành công",
                           statusCode: 200
                       ));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
        }
    }
}
