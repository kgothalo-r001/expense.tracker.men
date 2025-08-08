using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation;

public class SessionService : ISessionService
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IAuthenticatedUserHelper _userHelper;
    private readonly ITokenService _tokenService;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        IUserSessionRepository sessionRepository,
        IAuthenticatedUserHelper userHelper,
        ITokenService tokenService,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _userHelper = userHelper;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<UserSession> CreateSessionAsync(Guid userId, string token)
    {
        try
        {
            var session = new UserSession
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetJwtExpiryMinutes()),
                IsActive = true
            };

            var createdSession = await _sessionRepository.CreateSessionAsync(session);
            
            // Set session cookie for enhanced security
            _userHelper.SetSessionCookie(createdSession.Id.ToString());
            
            return createdSession;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateSessionAsync(string token)
    {
        try
        {
            // Validate both session and JWT token
            if (!await _sessionRepository.ValidateSessionAsync(token))
            {
                return false;
            }

            // Additional JWT token validation
            return _tokenService.ValidateJwtToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session validation failed for token: {TokenPrefix}", 
                token[..Math.Min(token.Length, 10)] + "...");
            return false;
        }
    }

    public async Task<User?> GetUserBySessionTokenAsync(string token)
    {
        try
        {
            return await _sessionRepository.GetUserBySessionTokenAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user by session token: {TokenPrefix}", 
                token[..Math.Min(token.Length, 10)] + "...");
            return null;
        }
    }

    public async Task<bool> DeactivateSessionAsync(string token)
    {
        try
        {
            _userHelper.ClearSessionCookie();
            return await _sessionRepository.DeactivateSessionAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deactivate session for token: {TokenPrefix}", 
                token[..Math.Min(token.Length, 10)] + "...");
            return false;
        }
    }

    public async Task<UserSession> RefreshSessionAsync(string oldToken, string newToken, Guid userId)
    {
        try
        {
            // Deactivate old session
            await _sessionRepository.DeactivateSessionAsync(oldToken);

            // Create new session
            return await CreateSessionAsync(userId, newToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh session for user: {UserId}", userId);
            throw;
        }
    }
}
