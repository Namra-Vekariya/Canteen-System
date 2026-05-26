using System.Net;
using System.Text.Json;
using CanteenSystem.Application.Common;
using CanteenSystem.Application.Common.Exceptions;   // ← ADD THIS

namespace CanteenSystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)                          // ← ADD THIS BLOCK
        {
            _logger.LogWarning("App exception [{StatusCode}]: {Message}", ex.StatusCode, ex.Message);
            await HandleExceptionAsync(context, (int)ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError,
                "An internal server error occurred. Please try again later.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string message,List<string>? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.FailureResponse(message,errors);
        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}