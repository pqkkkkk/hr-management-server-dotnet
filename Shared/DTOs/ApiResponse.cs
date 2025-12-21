namespace HrManagement.Api.Shared.DTOs;

/// <summary>
/// Represents an API error with error message.
/// </summary>
public record ApiError(
    string Error
);

/// <summary>
/// Standard API response wrapper for all endpoints.
/// Matches Spring Boot Response.ApiResponse structure.
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public record ApiResponse<T>(
    T? Data,
    bool Success,
    int StatusCode,
    string Message,
    ApiError? Error = null
)
{
    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new(data, true, 200, message, null);

    /// <summary>
    /// Creates a successful response with data and custom status code.
    /// </summary>
    public static ApiResponse<T> Ok(T data, int statusCode, string message = "Success")
        => new(data, true, statusCode, message, null);

    /// <summary>
    /// Creates a created (201) response.
    /// </summary>
    public static ApiResponse<T> Created(T data, string message = "Created successfully")
        => new(data, true, 201, message, null);

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static ApiResponse<T> Fail(int statusCode, string message, string? errorDetail = null)
        => new(default, false, statusCode, message, errorDetail != null ? new ApiError(errorDetail) : null);

    /// <summary>
    /// Creates a 400 Bad Request response.
    /// </summary>
    public static ApiResponse<T> BadRequest(string message, string? errorDetail = null)
        => Fail(400, message, errorDetail);

    /// <summary>
    /// Creates a 404 Not Found response.
    /// </summary>
    public static ApiResponse<T> NotFound(string message = "Resource not found")
        => Fail(404, message, null);

    /// <summary>
    /// Creates a 500 Internal Server Error response.
    /// </summary>
    public static ApiResponse<T> InternalError(string message = "An error occurred", string? errorDetail = null)
        => Fail(500, message, errorDetail);
}
