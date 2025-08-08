using System.Security.Claims;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace Expense.Tracker.Services.Services;

public interface ICurrentUserService
{
    Task<User?> GetCurrentUserAsync();
    Guid? GetCurrentUserId();
    string? GetCurrentUserToken();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserSessionRepository _userSessionRepository;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserSessionRepository userSessionRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userSessionRepository = userSessionRepository;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var token = GetCurrentUserToken();
        if (string.IsNullOrEmpty(token))
            return null;

        return await _userSessionRepository.GetUserBySessionTokenAsync(token);
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    public string? GetCurrentUserToken()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}
