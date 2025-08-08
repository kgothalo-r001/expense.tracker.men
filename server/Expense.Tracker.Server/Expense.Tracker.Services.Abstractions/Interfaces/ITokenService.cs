using System.Security.Claims;

namespace Expense.Tracker.Services.Abstractions.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateJwtToken(Guid userId, string username, string email);

    /// <summary>
    /// Validates a JWT token without checking session storage.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    bool ValidateJwtToken(string token);

    /// <summary>
    /// Extracts the claims principal from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    ClaimsPrincipal? GetPrincipalFromToken(string token);

    /// <summary>
    /// Gets the configured JWT token expiry time in minutes.
    /// </summary>
    /// <returns>The expiry time in minutes.</returns>
    int GetJwtExpiryMinutes();
}
