using System.Net;
using System.Text.Json;
using InternTrack.Business.Interfaces;

namespace InternTrack.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAppLogger logger)
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

            await HandleExceptionAsync(context, exception, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IAppLogger logger)
    {
        var traceId = context.TraceIdentifier;

        logger.LogError(
            exception,
            "Beklenmeyen hata oluştu. TraceId: {TraceId}",
            traceId);

        context.Response.Clear();

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,

            message = "Beklenmeyen bir hata oluştu.",

            statusCode = (int)HttpStatusCode.InternalServerError,

            traceId
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}
