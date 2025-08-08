using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Data;

namespace Expense.Tracker.Services.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public EfUserRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _context.Users
            .Where(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}

public class EfUserSessionRepository : IUserSessionRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public EfUserSessionRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession> CreateSessionAsync(UserSession session)
    {
        session.Id = Guid.NewGuid();
        session.CreatedAt = DateTime.UtcNow;

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<UserSession?> GetSessionByTokenAsync(string token)
    {
        return await _context.UserSessions
            .Include(s => s.User)
            .Where(s => s.Token == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeactivateSessionAsync(string token)
    {
        var session = await _context.UserSessions
            .Where(s => s.Token == token)
            .FirstOrDefaultAsync();

        if (session == null) return false;

        session.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateSessionAsync(string token)
    {
        return await _context.UserSessions
            .AnyAsync(s => s.Token == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<User?> GetUserBySessionTokenAsync(string token)
    {
        var session = await _context.UserSessions
            .Include(s => s.User)
            .Where(s => s.Token == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        return session?.User;
    }
}
