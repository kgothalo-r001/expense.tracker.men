using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Repositories;

public class EfCategoryRepositoryTests
{
    [Fact]
    public async Task CreateAsync_AddsCategoryAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateCategoryTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfCategoryRepository(context);
        var categoryId = Guid.NewGuid().ToString();
        var category = new Category { Id = categoryId, Name = "Food", Type = CategoryType.EXPENSE };
        var result = await repo.CreateAsync(category);
        result.Should().NotBeNull();
        result.Name.Should().Be("Food");
        context.Categories.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "GetCategoryTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfCategoryRepository(context);
        var categoryId = Guid.NewGuid().ToString();
        var category = new Category { Id = categoryId, Name = "Transport", Type = CategoryType.EXPENSE };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        var result = await repo.GetByIdAsync(categoryId);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Transport");
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteCategoryTest")
            .Options;
        using var context = new ExpenseTrackerDbContext(options);
        var repo = new EfCategoryRepository(context);
        var categoryId = Guid.NewGuid().ToString();
        var category = new Category { Id = categoryId, Name = "Bills", Type = CategoryType.EXPENSE };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        var result = await repo.DeleteAsync(categoryId);
        result.Should().BeTrue();
        context.Categories.Count().Should().Be(0);
    }
}
