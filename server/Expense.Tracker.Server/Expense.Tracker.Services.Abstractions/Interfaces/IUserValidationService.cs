namespace Expense.Tracker.Services.Abstractions.Interfaces;

public interface IUserValidationService
{
    /// <summary>
    /// Checks if a username is available.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if available, false otherwise.</returns>
    Task<bool> IsUsernameAvailableAsync(string username);

    /// <summary>
    /// Checks if an email is available.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if available, false otherwise.</returns>
    Task<bool> IsEmailAvailableAsync(string email);

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The password hash.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Suggests available usernames based on a base username.
    /// </summary>
    /// <param name="baseUsername">The base username to generate suggestions from.</param>
    /// <returns>A list of suggested available usernames.</returns>
    Task<List<string>> SuggestUsernamesAsync(string baseUsername);
}
