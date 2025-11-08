using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class JuryController : ControllerBase
    {
        private readonly IJuryService _juryService;
        public JuryController(IJuryService juryService) {
            _juryService = juryService;
        }
        [HttpGet(ApiEndpoints.Jury.GetAllByClubId)]
        public async Task<IActionResult> GetAllJuryByActivityId(int id,[FromQuery]string? Search)
        {
            try {
                var result = await _juryService.GetAllJuryByActivityIdAsync(id, Search);
                return Ok(new ResponseDto<List<JuryActivityResponseDto>>(result, "Lấy danh sách thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }   

        }
        [HttpPost(ApiEndpoints.Jury.ApiJury)]
        public async Task<IActionResult> CreateJury([FromBody] CreateJuryDto dto)
        {
            try
            {
                var result = await _juryService.CreateJury(dto);
                return Ok(new ResponseDto<List<JuryActivityResponseDto>>(result, "Tạo giám khảo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpDelete(ApiEndpoints.Jury.GetAllByClubId)]
        public async Task<IActionResult> DeleteJury(int id)
        {
            try
            {
                await _juryService.DeletedJury(id);
                return Ok(new ResponseDto<string>(null, "Xóa giám khảo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
