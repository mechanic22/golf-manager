namespace GolfManager.Shared.DTOs.Common;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// A human-readable message about the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The response data (null if request failed)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of error messages (empty if request succeeded)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse<T> SuccessResponse(string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = default,
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, params string[] errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors.ToList()
        };
    }

    /// <summary>
    /// Creates an error response with a single error
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = new List<string> { error }
        };
    }
}

