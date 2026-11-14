using System.Net;
using System.Text.Json;
using SecureNoteApp.Application.Common.Exceptions;

namespace SecureNoteApp.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, forbidden.Message),
            BadRequestException badRequest => (HttpStatusCode.BadRequest, badRequest.Message),
            ConflictException conflict => (HttpStatusCode.Conflict, conflict.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected internal server error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled Exception ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            statusCode = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(json);
    }
}
