using System.Net;
using System.Text.Json;

namespace InternTrack.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleExceptionAsync(
                context,
                exception
            );
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId =
            context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Beklenmeyen hata oluştu. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            traceId
        );

        context.Response.Clear();

        context.Response.StatusCode =
            (int)HttpStatusCode.InternalServerError;

        context.Response.ContentType =
            "application/json";

        var response =
            new
            {
                success = false,

                message =
                    "Beklenmeyen bir hata oluştu.",

                statusCode =
                    (int)HttpStatusCode.InternalServerError,

                traceId
            };

        var json =
            JsonSerializer.Serialize(
                response
            );

        await context.Response.WriteAsync(
            json
        );
    }
}