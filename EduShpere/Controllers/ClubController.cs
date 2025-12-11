using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Domain;
using EduShpere.Shared;
using EduShpere.Application.DTOs.SearchDto;

namespace EduShpere.Controllers
{
    
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;
        private readonly IHttpContextService _httpContextService;
        public ClubController(IClubService clubService, IHttpContextService httpContextService)
        {
            _clubService = clubService;
            _httpContextService = httpContextService;
        }
        [HttpGet(ApiEndpoints.Club.Clubs)]
        public async Task<IActionResult> GetAllClub([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubService.GetAllAsync(user, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubResponseDto>>(
                       response,
                       message: "Lấy danh sách câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpGet(ApiEndpoints.Club.GetClubById)]
        public async Task<IActionResult> GetClubById( int id)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _clubService.GetClubByID(id,user);
            return Ok(new ResponseDto<ClubResponseDto>(
                       response,
                       message: "Lấy thông tin câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpPut(ApiEndpoints.Club.Clubs)]
        public async Task<IActionResult> UpdateClub([FromBody] UpdateClubDto dto)
        {
            var response = await _clubService.UpdateClub(dto);
            return Ok(new ResponseDto<ClubResponseDto>(
                       response,
                       message: "Cập nhật câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpPost(ApiEndpoints.Club.Clubs)]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            var response = await _clubService.CreateClub(dto);
            return Ok(new ResponseDto<ClubResponseDto>(
                       response,
                       message: "Tạo câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpDelete(ApiEndpoints.Club.GetClubById)]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var result = await _clubService.DeleteClub(id);
            return Ok(new ResponseDto<bool>(
                       result,
                       message: "Xóa câu lạc bộ thành công",
                       statusCode: 200
                   ));
        }
        [HttpGet(ApiEndpoints.Club.Categories)]
        public async Task<IActionResult> GetAllClubCategory()
        {
            try{
                var response = await _clubService.GetAllClubCategory();
                return Ok(new ResponseDto<IEnumerable<ClubCategory>>(
                           response,
                           message: "Lấy danh sách thể loại câu lạc bộ thành công",
                           statusCode: 200
                       ));
            }
            catch(BadRequestException error)
            {
                throw new BadRequestException(error.Message);
            }
            
        }
        [HttpGet(ApiEndpoints.Club.SearchUsers)]
        public async Task<IActionResult> GetAllUserByRole([FromQuery] int role, [FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var response = await _clubService.GetAllAsync(role, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<UserSearchResultDto>>(
                       response,
                       message: "Lấy danh sách người dùng theo vai trò thành công",
                       statusCode: 200
                   ));
        }
    }
}
