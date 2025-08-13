using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Tests.Services.Repositories;

public class EfTagRepositoryTests
{
    [Fact]
    public async Task CreateAsync_AddsTagAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateTagTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfTagRepository(context);
        var tagId = Guid.NewGuid().ToString();
        var tag = new Tag { Id = tagId, Name = "Groceries", Color = "red" };
        var result = await repo.CreateAsync(tag);
        result.Should().NotBeNull();
        result.Name.Should().Be("Groceries");
        context.Tags.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTag()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "GetTagTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfTagRepository(context);
        var tagId = Guid.NewGuid().ToString();
        var tag = new Tag { Id = tagId, Name = "Bills", Color = "blue" };
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        var result = await repo.GetByIdAsync(tagId);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bills");
    }

    [Fact]
    public async Task DeleteAsync_RemovesTag()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteTagTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfTagRepository(context);
        var tagId = Guid.NewGuid().ToString();
        var tag = new Tag { Id = tagId, Name = "Transport", Color = "green" };
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        var result = await repo.DeleteAsync(tagId);
        result.Should().BeTrue();
        context.Tags.Count().Should().Be(0);
    }
}
