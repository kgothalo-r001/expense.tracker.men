using System.Security.Claims;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    /// <summary>
    /// Service to get information about the currently authenticated user
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Get the ID of the currently authenticated user
        /// </summary>
        /// <returns>User ID as Guid, or null if no user is authenticated</returns>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Get the username of the currently authenticated user
        /// </summary>
        /// <returns>Username as string, or null if no user is authenticated</returns>
        string? GetCurrentUsername();

        /// <summary>
        /// Check if a user is currently authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Get all claims for the current user
        /// </summary>
        /// <returns>Claims principal or null if not authenticated</returns>
        ClaimsPrincipal? GetCurrentUser();
    }
}
