using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Tests.Services.Repositories;

public class EfUserSessionRepositoryTests
{
    [Fact]
    public async Task CreateSessionAsync_AddsSessionAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateSessionTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserSessionRepository(context);
        var session = new UserSession { UserId = Guid.NewGuid(), Token = "token1", IsActive = true, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        var result = await repo.CreateSessionAsync(session);
        result.Should().NotBeNull();
        result.Token.Should().Be("token1");
        context.UserSessions.Count().Should().Be(1);
    }

    //[Fact]
    //public async Task GetSessionByTokenAsync_ReturnsSession()
    //{
    //    var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
    //        .UseInMemoryDatabase(databaseName: "GetSessionByTokenTest")
    //        .Options;

    //    using (var context = new ExpenseTrackerDbContext(options))
    //    {
    //        var session = new UserSession { UserId = Guid.NewGuid(), Token = "token2", IsActive = true, ExpiresAt = DateTime.UtcNow.AddHours(1) };
    //        context.UserSessions.Add(session);
    //        await context.SaveChangesAsync();
    //    }

    //    using (var context = new ExpenseTrackerDbContext(options))
    //    {
    //        var repo = new EfUserSessionRepository(context);
    //        var result = await repo.GetSessionByTokenAsync("token2");
    //        result.Should().NotBeNull();
    //        result!.Token.Should().Be("token2");
    //    }
    //}

    [Fact]
    public async Task DeactivateSessionAsync_DeactivatesSession()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "DeactivateSessionTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserSessionRepository(context);
        var session = new UserSession { UserId = Guid.NewGuid(), Token = "token3", IsActive = true, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        context.UserSessions.Add(session);
        await context.SaveChangesAsync();
        var result = await repo.DeactivateSessionAsync("token3");
        result.Should().BeTrue();
        var updated = await repo.GetSessionByTokenAsync("token3");
        updated.Should().BeNull();
    }
}
