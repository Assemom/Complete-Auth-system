using System.Text.Json;
using App.Shared.Constants;
using App.Shared.Exceptions;
using App.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Api.Middlewares;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, logLevel) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, LogLevel.Warning),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, LogLevel.Warning),
            ForbiddenException => (StatusCodes.Status403Forbidden, LogLevel.Warning),
            NotFoundException => (StatusCodes.Status404NotFound, LogLevel.Warning),
            ConflictException => (StatusCodes.Status409Conflict, LogLevel.Warning),
            ServerException => (StatusCodes.Status500InternalServerError, LogLevel.Error),
            _ => (StatusCodes.Status500InternalServerError, LogLevel.Error)
        };

        var message = exception is AppException || !_environment.IsProduction()
            ? exception.Message
            : ErrorMessages.UnhandledException;

        _logger.Log(logLevel, exception, "Unhandled exception. Path: {Path}. StatusCode: {StatusCode}. Message: {Message}", context.Request.Path, statusCode, message);

        var response = new ErrorResponse
        {
            Message = message,
            StatusCode = statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = JsonSerializer.Serialize(response, SerializerOptions);
        await context.Response.WriteAsync(payload);
    }
}
