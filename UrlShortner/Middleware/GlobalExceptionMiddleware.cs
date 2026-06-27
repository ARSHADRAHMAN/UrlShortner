using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace UrlShortner.Middleware;

/// <summary>
/// Global exception middleware for handling unhandled exceptions
/// Returns standardized ProblemDetails responses and logs exceptions
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger = Log.ForContext<GlobalExceptionMiddleware>();

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Invoke the middleware to handle exceptions
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or retrieve correlation ID
        var correlationId = context.TraceIdentifier;
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationIdHeader))
        {
            correlationId = correlationIdHeader.ToString();
        }

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        try
        {
            _logger.Information(
                "Incoming request: {HttpMethod} {Path} from {RemoteIpAddress} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                correlationId);

            var startTime = DateTime.UtcNow;

            await _next(context);

            var executionTime = DateTime.UtcNow - startTime;
            _logger.Information(
                "Outgoing response: {StatusCode} {Path} - Execution time: {ExecutionTimeMs}ms [CorrelationId: {CorrelationId}]",
                context.Response.StatusCode,
                context.Request.Path,
                executionTime.TotalMilliseconds,
                correlationId);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning(
                "Request cancelled: {HttpMethod} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            await WriteProblemDetailsAsync(
                context,
                (int)HttpStatusCode.RequestTimeout,
                "Request Timeout",
                "The request was cancelled or timed out.",
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.Fatal(
                ex,
                "Unhandled exception in request: {HttpMethod} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    /// <summary>
    /// Handle exceptions and return ProblemDetails response
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        // Handle specific exception types for more detailed responses
        if (exception is ArgumentException argEx)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return WriteProblemDetailsAsync(context, response.StatusCode, "Validation Error", argEx.Message, correlationId);
        }
        else if (exception is InvalidOperationException invEx)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return WriteProblemDetailsAsync(context, response.StatusCode, "Invalid Operation", invEx.Message, correlationId);
        }
        else if (exception is NotImplementedException)
        {
            response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return WriteProblemDetailsAsync(context, response.StatusCode, "Not Implemented", "This feature is not yet implemented.", correlationId);
        }
        else if (exception is UnauthorizedAccessException)
        {
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return WriteProblemDetailsAsync(context, response.StatusCode, "Unauthorized", "Access denied.", correlationId);
        }
        else if (exception is KeyNotFoundException)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return WriteProblemDetailsAsync(context, response.StatusCode, "Resource Not Found", "The requested resource was not found.", correlationId);
        }

        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return WriteProblemDetailsAsync(
            context,
            (int)HttpStatusCode.InternalServerError,
            "Internal Server Error",
            "An unexpected error has occurred. Please contact support with the correlation ID.",
            correlationId);
    }

    /// <summary>
    /// Write ProblemDetails response to the HTTP response
    /// </summary>
    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string correlationId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = new Dictionary<string, object?>
            {
                { "correlationId", correlationId },
                { "timestamp", DateTime.UtcNow }
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

/// <summary>
/// Extension methods for registering global exception middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Add global exception middleware to the pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
