using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces;

public interface ISessionService
{
    /// <summary>
    /// Creates a new user session.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The JWT token for the session.</param>
    /// <returns>The created user session.</returns>
    Task<UserSession> CreateSessionAsync(Guid userId, string token);

    /// <summary>
    /// Validates a session token.
    /// </summary>
    /// <param name="token">The session token to validate.</param>
    /// <returns>True if the session is valid, false otherwise.</returns>
    Task<bool> ValidateSessionAsync(string token);

    /// <summary>
    /// Gets a user by their session token.
    /// </summary>
    /// <param name="token">The session token.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetUserBySessionTokenAsync(string token);

    /// <summary>
    /// Deactivates a user session.
    /// </summary>
    /// <param name="token">The session token to deactivate.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DeactivateSessionAsync(string token);

    /// <summary>
    /// Refreshes a user session by deactivating the old one and creating a new one.
    /// </summary>
    /// <param name="oldToken">The old session token.</param>
    /// <param name="newToken">The new JWT token.</param>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The new user session.</returns>
    Task<UserSession> RefreshSessionAsync(string oldToken, string newToken, Guid userId);
}
