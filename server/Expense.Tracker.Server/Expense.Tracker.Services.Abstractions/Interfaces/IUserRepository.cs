using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
}

public interface IUserSessionRepository
{
    Task<UserSession> CreateSessionAsync(UserSession session);
    Task<UserSession?> GetSessionByTokenAsync(string token);
    Task<bool> DeactivateSessionAsync(string token);
    Task<bool> ValidateSessionAsync(string token);
    Task<User?> GetUserBySessionTokenAsync(string token);
}
