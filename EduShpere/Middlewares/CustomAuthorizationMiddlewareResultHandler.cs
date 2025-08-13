using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Text.Json;
using EduShpere.Application;

namespace EduShpere;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly ILogger<CustomAuthorizationMiddlewareResultHandler> _logger;

    public CustomAuthorizationMiddlewareResultHandler(ILogger<CustomAuthorizationMiddlewareResultHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var request = context.Request;
        var response = context.Response;

        response.ContentType = "application/json";

        var statusCode = response.StatusCode;

        _logger.LogInformation($"Message from AuthorizationMiddlewareResultHandler with StatusCode = {statusCode}");

        // If the authorization was forbidden and the resource had a specific requirement,
        // provide a custom 404 response.
        if (authorizeResult.Forbidden)
        {
            // Return a 404 to make it appear as if the resource doesn't exist.
            statusCode = StatusCodes.Status404NotFound;
        }

        if (statusCode == StatusCodes.Status200OK)
        {
            // Fall back to the default implementation.
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
        else
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var message = statusCode == StatusCodes.Status404NotFound ? "Not Found" : "Unauthorized";

            var result = JsonSerializer.Serialize(new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = message,
                Path = request.Path.Value,
                Timestamp = DateTime.UtcNow
            }, serializeOptions);

            // await Results.Content(result, "application/json").ExecuteAsync(context);
            await response.WriteAsync(result);
        }
    }
}

