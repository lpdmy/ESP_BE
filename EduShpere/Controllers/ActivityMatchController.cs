using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityMatchDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using EduShpere.Middlewares;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class ActivityMatchController : BaseController
    {
        private readonly IActivityMatchService _service;

        public ActivityMatchController(IActivityMatchService service)
        {
            _service = service;
        }

        [HttpPost(ApiEndpoints.ActivityMatch.GenerateBracket)]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GenerateBracket([FromBody] GenerateBracketDto dto)
        {
            if (dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            var result = await _service.GenerateSingleEliminationBracketAsync(dto);
            return Ok(new ResponseDto<BracketResponseDto>(result, "Tạo bracket thành công", 200));
        }

        [HttpGet(ApiEndpoints.ActivityMatch.GetBracket)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetBracket([FromQuery] int activityId, [FromQuery] int sportId, [FromQuery] int? grade = null)
        {
            if (activityId <= 0 || sportId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.GetBracketByActivityAsync(activityId, sportId, grade);
                return Ok(new ResponseDto<BracketResponseDto>(result, "Lấy bracket thành công", 200));
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
        }

        [HttpGet(ApiEndpoints.ActivityMatch.GetMatchesByRound)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMatchesByRound([FromRoute] int round, [FromQuery] int activityId, [FromQuery] int sportId, [FromQuery] int? grade = null)
        {
            if (activityId <= 0 || sportId <= 0 || round <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            var result = await _service.GetMatchesByRoundAsync(activityId, sportId, round, grade);
            return Ok(new ResponseDto<IEnumerable<MatchResponseDto>>(result, "Lấy danh sách trận đấu thành công", 200));
        }

        [HttpGet(ApiEndpoints.ActivityMatch.GetMatchById)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMatchById([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.GetMatchByIdAsync(id);
                return Ok(new ResponseDto<MatchResponseDto>(result, "Lấy thông tin trận đấu thành công", 200));
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
        }

        [HttpPost(ApiEndpoints.ActivityMatch.CreateMatch)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchDto dto)
        {
            if (dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.CreateMatchAsync(dto);
                return Ok(new ResponseDto<MatchResponseDto>(result, "Tạo trận đấu thành công", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
            }
        }

        [HttpPut(ApiEndpoints.ActivityMatch.UpdateMatchResult)]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateMatchResult([FromRoute] int id, [FromBody] UpdateMatchResultDto dto)
        {
            if (id <= 0 || dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.UpdateMatchResultAsync(id, dto);
                return Ok(new ResponseDto<MatchResponseDto>(result, "Cập nhật kết quả trận đấu thành công", 200));
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
            }
        }

        [HttpPut(ApiEndpoints.ActivityMatch.UpdateMatch)]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateMatch([FromRoute] int id, [FromBody] UpdateMatchDto dto)
        {
            if (id <= 0 || dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.UpdateMatchAsync(id, dto);
                return Ok(new ResponseDto<MatchResponseDto>(result, "Cập nhật thông tin trận đấu thành công", 200));
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
            }
        }

        [HttpDelete(ApiEndpoints.ActivityMatch.DeleteBracket)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBracket([FromQuery] int activityId, [FromQuery] int sportId, [FromQuery] int? grade = null)
        {
            if (activityId <= 0 || sportId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            var result = await _service.DeleteBracketAsync(activityId, sportId, grade);
            return Ok(new ResponseDto<bool>(result, "Xóa bracket thành công", 200));
        }

        [HttpDelete(ApiEndpoints.ActivityMatch.DeleteMatch)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMatch([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            try
            {
                var result = await _service.DeleteMatchAsync(id);
                return Ok(new ResponseDto<bool>(result, "Xóa trận đấu thành công", 200));
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
        }

        [HttpGet(ApiEndpoints.ActivityMatch.GetEligibleClassGroups)]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetEligibleClassGroups([FromQuery] int activityId, [FromQuery] int sportId)
        {
            if (activityId <= 0 || sportId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));

            var result = await _service.GetEligibleClassGroupsAsync(activityId, sportId);
            return Ok(new ResponseDto<IEnumerable<EligibleClassGroupsByGradeDto>>(result, "Lấy danh sách lớp đủ điều kiện thành công", 200));
        }
    }
}

