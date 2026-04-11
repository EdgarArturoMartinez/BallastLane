using System.Net;
using System.Text.Json;
using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Api.Middleware;

/// <summary>
/// Global exception handler middleware.
///
/// Pattern: Chain of Responsibility (ASP.NET Core middleware pipeline).
/// SOLID: SRP — centralises error-to-HTTP mapping; controllers stay clean.
///
/// Maps:
///   DomainException          → 400 Bad Request  (business rule violations)
///   UnauthorizedAccessException → 401 Unauthorized
///   KeyNotFoundException     → 404 Not Found
///   Everything else          → 500 Internal Server Error (message hidden in prod)
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            DomainException             => (HttpStatusCode.BadRequest,           ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,         ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,             ex.Message),
            _                           => (HttpStatusCode.InternalServerError,  "An unexpected error occurred.")
        };

        var correlationId = context.Items.TryGetValue("CorrelationId", out var cid) ? cid?.ToString() : "n/a";

        _logger.LogError(ex,
            "Unhandled exception [{StatusCode}] on {Method} {Path} CorrelationId={CorrelationId}",
            (int)statusCode, context.Request.Method, context.Request.Path, correlationId);

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            status        = (int)statusCode,
            error         = message,
            correlationId,
            // Only include stack trace in development to avoid leaking internal details (OWASP A05)
            detail = _env.IsDevelopment() ? ex.ToString() : null
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
