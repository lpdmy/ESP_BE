using EduShpere.Application;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult GetValidationErrorResponse()
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            return BadRequest(new ResponseDto<string>(
                null, 
                firstError ?? ErrorMessages.UserProfile.ValidationFailed, 
                400
            ));
        }

        protected IActionResult GetValidationErrorResponse(string customMessage)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            return BadRequest(new ResponseDto<string>(
                null, 
                $"{customMessage}: {firstError}", 
                400
            ));
        }

        protected IActionResult GetValidationErrorResponse(string customMessage, string errorKey)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            return BadRequest(new ResponseDto<string>(
                null, 
                $"{customMessage}: {firstError}", 
                400
            ));
        }
    }
}
