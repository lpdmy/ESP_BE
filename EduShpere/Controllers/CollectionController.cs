using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Domain.Models;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _Service;
        private readonly IHttpContextService _httpContextService;
        public CollectionController(ICollectionService service, IHttpContextService httpContextService)
        {
            _Service = service;
            _httpContextService = httpContextService;
        }
        [HttpPost(ApiEndpoints.Collection.Collections)]
        public async Task<IActionResult> CreateCollection([FromBody]CreateCollectionDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try {
                var result = await _Service.CreateCollectionResponse(user, dto);

                return Ok(new ResponseDto<CollectionResponseDto>(result, "tạo bộ sưu tập thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpGet(ApiEndpoints.Collection.GetCollectionByUser)]
        public async Task<IActionResult> GetCollectionByUser([FromQuery] PaginationRequestDto paginationRequest)
        {
            try
            {
                var user = await _httpContextService.GetAppUserAndThrow();
                var result = await _Service.GetAllCollectionByUserAsync(user, paginationRequest);

                return Ok(new ResponseDto<PaginationResponseDto<CollectionResponseDto>>(
                    result,
                    message: "Lấy bộ sưu tập thành công",
                    statusCode: 200
                ));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete(ApiEndpoints.Collection.Collections)]
        public async Task<IActionResult> DeleteCollection([FromRoute] int id)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _Service.DeleteCollection(id);
                return Ok(new ResponseDto<bool>(result, "xóa bộ sưu tập thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpPut(ApiEndpoints.Collection.Collections)]
        public async Task<IActionResult> UpdateCollection([FromBody] UpdateCollectionDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _Service.UpdateCollection(dto);
                return Ok(new ResponseDto<bool>(result, "cập nhật bộ sưu tập thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpPost(ApiEndpoints.Collection.AddCollectionIteam)]
        public async Task<IActionResult> AddCollectionIteam([FromBody] AddCollectionIteamDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _Service.AddCollectionIteam(dto, user);
                return Ok(new ResponseDto<CollectionItem>(result, "thêm mục vào bộ sưu tập thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
