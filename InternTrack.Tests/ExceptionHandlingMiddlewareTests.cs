using System.Text.Json;
using InternTrack.Api.Middleware;
using InternTrack.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;

namespace InternTrack.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldLogUnexpectedFailureWithoutRequestOrExceptionSecrets()
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "test-trace-id";
        context.Request.Path = "/private-path-value";
        context.Request.QueryString = new QueryString("?token=private-query-token");
        context.Request.Headers.Authorization = "Bearer private-jwt";
        using var body = new MemoryStream();
        context.Response.Body = body;
        var middleware = new ExceptionHandlingMiddleware(_ =>
            throw new InvalidOperationException("private-exception-value"));

        await middleware.InvokeAsync(context, logger);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        body.Position = 0;
        using var response = await JsonDocument.ParseAsync(body);
        Assert.False(response.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Beklenmeyen bir hata oluştu.", response.RootElement.GetProperty("message").GetString());
        Assert.Equal(500, response.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("test-trace-id", response.RootElement.GetProperty("traceId").GetString());

        var json = output.ToString();
        using var entry = JsonDocument.Parse(json);
        Assert.Equal("Error", entry.RootElement.GetProperty("Level").GetString());
        Assert.Equal("test-trace-id", entry.RootElement.GetProperty("Properties").GetProperty("TraceId").GetString());
        Assert.DoesNotContain("private-", json);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLeaveSuccessfulRequestsUnchangedWithoutLogging()
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);
        var context = new DefaultHttpContext();
        var middleware = new ExceptionHandlingMiddleware(httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, logger);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Empty(output.ToString());
    }
}
