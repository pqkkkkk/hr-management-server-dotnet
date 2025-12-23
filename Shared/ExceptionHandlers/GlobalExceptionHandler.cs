using Microsoft.AspNetCore.Diagnostics;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Shared.ExceptionHandlers;

/// <summary>
/// Global exception handler that catches all unhandled exceptions
/// and returns a standardized ApiResponse format.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        // Map exception types to HTTP status codes and error codes
        var (statusCode, errorCode) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND"),
            ArgumentNullException => (StatusCodes.Status400BadRequest, "BAD_REQUEST"),
            ArgumentException => (StatusCodes.Status400BadRequest, "BAD_REQUEST"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "CONFLICT"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED"),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(statusCode, exception.Message, errorCode);

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
