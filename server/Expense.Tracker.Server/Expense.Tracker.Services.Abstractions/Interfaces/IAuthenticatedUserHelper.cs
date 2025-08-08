using System;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface IAuthenticatedUserHelper
    {
        /// <summary>
        /// Gets the current authenticated user ID, throwing UnauthorizedAccessException if not authenticated
        /// </summary>
        /// <returns>Current user ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        Guid GetAuthenticatedUserId();
        
        /// <summary>
        /// Gets the current authenticated user ID with session validation
        /// </summary>
        /// <returns>Current user ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or session is invalid</exception>
        Task<Guid> GetAuthenticatedUserIdAsync();
        
        /// <summary>
        /// Validates that the requesting user can access resources for the specified user ID
        /// </summary>
        /// <param name="requestedUserId">The user ID being requested</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when access is denied</exception>
        void ValidateUserAccess(Guid requestedUserId);
        
        /// <summary>
        /// Gets session ID from HTTP context
        /// </summary>
        /// <returns>Session ID if available</returns>
        string? GetSessionId();
        
        /// <summary>
        /// Sets user session cookie with expiration
        /// </summary>
        /// <param name="sessionId">Session ID to store</param>
        void SetSessionCookie(string sessionId);
        
        /// <summary>
        /// Clears user session cookie
        /// </summary>
        void ClearSessionCookie();
    }
}
