using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _service;
        public SubmissionController(ISubmissionService service)
        {
            _service = service;
        }
        [HttpGet(ApiEndpoints.Submission.GetAllByActivityId)]
        public async Task<IActionResult> GetAllByActivityId([FromQuery]PaginationRequestDto dto,int Id,string ? search)
        {
            try {
                var result = await _service.GetAllSubmissionByActivityId(Id, dto, search);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách thành công", 200));
            }
            catch(BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
