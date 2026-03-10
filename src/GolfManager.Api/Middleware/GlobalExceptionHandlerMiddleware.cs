using System.Net;
using System.Text.Json;
using GolfManager.Shared.DTOs.Common;

namespace GolfManager.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns standardized error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = GetUserFriendlyMessage(exception),
            Errors = new List<string>()
        };

        // Determine status code based on exception type
        context.Response.StatusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        // Add detailed error information in development
        if (_environment.IsDevelopment())
        {
            response.Errors.Add(exception.Message);
            if (exception.InnerException != null)
            {
                response.Errors.Add($"Inner Exception: {exception.InnerException.Message}");
            }
            response.Errors.Add($"Stack Trace: {exception.StackTrace}");
        }
        else
        {
            // In production, only add generic error message
            response.Errors.Add("An error occurred while processing your request.");
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid request parameters.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            InvalidOperationException => "The operation could not be completed.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}

