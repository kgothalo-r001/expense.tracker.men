using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Expense.Tracker.Application.Middleware;

/// <summary>
/// Middleware to extract JWT tokens from cookies and add them to the Authorization header
/// for compatibility with the default JWT authentication handler.
/// </summary>
public class CookieJwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CookieJwtMiddleware> _logger;
    private const string AuthCookieName = "ExpenseTracker.Auth";

    public CookieJwtMiddleware(RequestDelegate next, ILogger<CookieJwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if there's already an Authorization header
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var token = context.Request.Cookies[AuthCookieName];
            
            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the CookieJwtMiddleware
/// </summary>
public static class CookieJwtMiddlewareExtensions
{
    public static IApplicationBuilder UseCookieJwt(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CookieJwtMiddleware>();
    }
}
