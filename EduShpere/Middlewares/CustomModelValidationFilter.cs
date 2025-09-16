using EduShpere.Application;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Middlewares
{
    public class CustomModelValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Manually validate all action parameters
            foreach (var parameter in context.ActionDescriptor.Parameters)
            {
                if (context.ActionArguments.TryGetValue(parameter.Name, out var argument) && argument != null)
                {
                    var validationContext = new ValidationContext(argument);
                    var validationResults = new List<ValidationResult>();
                    
                    if (!Validator.TryValidateObject(argument, validationContext, validationResults, true))
                    {
                        foreach (var validationResult in validationResults)
                        {
                            foreach (var memberName in validationResult.MemberNames)
                            {
                                context.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation failed");
                            }
                        }
                    }
                }
            }

            if (!context.ModelState.IsValid)
            {
                var firstError = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                var response = new ResponseDto<string>(
                    null,
                    firstError ?? ErrorMessages.UserProfile.ValidationFailed,
                    400
                );

                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}
