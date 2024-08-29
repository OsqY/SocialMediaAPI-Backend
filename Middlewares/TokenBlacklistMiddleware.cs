using SocialMediaAPI.Services;

namespace SocialMediaAPI.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenBlacklistService _tokenBlacklistService;

    public TokenBlacklistMiddleware(
        RequestDelegate next,
        TokenBlacklistService tokenBlacklistService
    )
    {
        _next = next;
        _tokenBlacklistService = tokenBlacklistService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (
            !string.IsNullOrEmpty(token)
            && await _tokenBlacklistService.IsTokenBlacklistedAsync(token)
        )
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token is no longer valid.");
            return;
        }

        await _next(context);
    }
}
