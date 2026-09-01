namespace InternTrack.Api.Middleware;

public class TokenCookieMiddleware
{
    private readonly RequestDelegate _next;

    public TokenCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken =
            context.Request.Cookies["accessToken"];

        if (
            !string.IsNullOrWhiteSpace(accessToken) &&
            !context.Request.Headers.ContainsKey(
                "Authorization"
            )
        )
        {
            context.Request.Headers.Authorization =
                $"Bearer {accessToken}";
        }

        await _next(context);
    }
}