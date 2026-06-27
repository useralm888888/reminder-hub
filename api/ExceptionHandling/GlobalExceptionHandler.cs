using Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.ExceptionHandling;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, logLevel) = MapException(exception);

        if (logLevel == LogLevel.Error)
        {
            logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            logger.LogDebug(exception, "Request failed with {StatusCode}", statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = environment.IsDevelopment() && logLevel == LogLevel.Error
                ? exception.Message
                : null
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, LogLevel LogLevel) MapException(Exception exception)
    {
        return exception switch
        {
            ReminderNotFoundException ex => (
                StatusCodes.Status404NotFound,
                ex.Message,
                LogLevel.Information),
            ReminderNotEditableException ex => (
                StatusCodes.Status409Conflict,
                ex.Message,
                LogLevel.Information),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "The reminder was modified by another process.",
                LogLevel.Information),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                LogLevel.Error)
        };
    }
}
