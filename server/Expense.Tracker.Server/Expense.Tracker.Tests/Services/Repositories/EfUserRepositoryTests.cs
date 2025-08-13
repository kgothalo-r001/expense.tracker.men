using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Tests.Services.Repositories;

public class EfUserRepositoryTests
{
    [Fact]
    public async Task CreateUserAsync_AddsUserAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateUserTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserRepository(context);
        var user = new User { Username = "user1", Email = "user1@email.com", IsActive = true };
        var result = await repo.CreateUserAsync(user);
        result.Should().NotBeNull();
        result.Username.Should().Be("user1");
        context.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "GetUserByIdTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@email.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var result = await repo.GetUserByIdAsync(user.Id);
        result.Should().NotBeNull();
        result!.Username.Should().Be("user2");
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_ReturnsFalseIfExists()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "UsernameAvailableTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserRepository(context);
        var user = new User { Username = "user3", Email = "user3@email.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var result = await repo.IsUsernameAvailableAsync("user3");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_ReturnsFalseIfExists()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "EmailAvailableTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfUserRepository(context);
        var user = new User { Username = "user4", Email = "user4@email.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var result = await repo.IsEmailAvailableAsync("user4@email.com");
        result.Should().BeFalse();
    }
}
