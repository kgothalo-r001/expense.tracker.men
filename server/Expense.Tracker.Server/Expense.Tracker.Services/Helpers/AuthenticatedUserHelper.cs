using Microsoft.AspNetCore.Http;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Helpers;
using System.Security.Claims;

namespace Expense.Tracker.Services.Helpers
{
    public class AuthenticatedUserHelper : IAuthenticatedUserHelper
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserSessionRepository _sessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(20);
        
        // Cache for current request to avoid multiple DB calls
        private Guid? _cachedUserId;
        private DateTime? _cacheExpiry;
        
        public AuthenticatedUserHelper(
            ICurrentUserService currentUserService,
            IUserSessionRepository sessionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _currentUserService = currentUserService;
            _sessionRepository = sessionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetAuthenticatedUserId()
        {
            // Check if we have a cached value that's still valid
            if (_cachedUserId.HasValue && _cacheExpiry.HasValue && DateTime.UtcNow < _cacheExpiry.Value)
            {
                return _cachedUserId.Value;
            }

            var userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Cache the result for this request (short-lived cache)
            _cachedUserId = userId.Value;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(1); // Cache for 1 minute per request
            
            return userId.Value;
        }

        public async Task<Guid> GetAuthenticatedUserIdAsync()
        {
            return GetAuthenticatedUserId();
        }

        public void ValidateUserAccess(Guid requestedUserId)
        {
            var currentUserId = GetAuthenticatedUserId();
            
            if (currentUserId != requestedUserId)
            {
                throw new UnauthorizedAccessException("You can only access your own resources.");
            }
        }

        public string? GetSessionId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;
            
            // Try to get session ID from cookie first
            if (httpContext.Request.Cookies.TryGetValue("ExpenseTracker_SessionId", out var cookieSessionId))
            {
                return cookieSessionId;
            }
            
            // Fallback to Authorization header or claims
            var sessionClaim = httpContext.User.FindFirst("session_id");
            return sessionClaim?.Value;
        }

        public void SetSessionCookie(string sessionId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;
            
            var cookieOptions = CookieHelper.GetSecureCookieOptions();
            cookieOptions.Expires = DateTime.UtcNow.Add(_sessionTimeout);
            
            httpContext.Response.Cookies.Append("ExpenseTracker_SessionId", sessionId, cookieOptions);
        }

        public void ClearSessionCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;
            
            httpContext.Response.Cookies.Append("ExpenseTracker_SessionId", "", CookieHelper.GetExpiredCookieOptions());
        }
    }
}
