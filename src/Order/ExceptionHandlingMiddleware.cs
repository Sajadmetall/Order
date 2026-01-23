using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Order.Middleware;

public sealed class ExceptionHandlingMiddleware
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
            if (context.Response.HasStarted)
            {
                // Can't write ProblemDetails once response has started
                _logger.LogWarning(ex, "Response has already started; cannot write error response.");
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var cid)
            ? cid?.ToString()
            : null;

        ProblemDetails problem;
        int statusCode;

        switch (ex)
        {
            case ValidationException ve:
                statusCode = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };

                problem.Extensions["errors"] = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                _logger.LogWarning(ex, "Validation failed");
                break;

            case NotFoundException nfe:
                statusCode = StatusCodes.Status404NotFound;
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "Not found",
                    Detail = nfe.Message,
                    Instance = context.Request.Path
                };
                _logger.LogWarning(ex, "Not found");
                break;

            case ArgumentException:
                statusCode = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = ReasonPhrases.GetReasonPhrase(statusCode),
                    Detail = ex.Message,
                    Instance = context.Request.Path
                };
                _logger.LogWarning(ex, "Bad request");
                break;

            case InvalidOperationException:
                statusCode = StatusCodes.Status409Conflict;
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = ReasonPhrases.GetReasonPhrase(statusCode),
                    Detail = ex.Message,
                    Instance = context.Request.Path
                };
                _logger.LogWarning(ex, "Business rule violation");
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = ReasonPhrases.GetReasonPhrase(statusCode),
                    Detail = "An unexpected error occurred.",
                    Instance = context.Request.Path
                };
                _logger.LogError(ex, "Unhandled exception");
                break;
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
            problem.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
