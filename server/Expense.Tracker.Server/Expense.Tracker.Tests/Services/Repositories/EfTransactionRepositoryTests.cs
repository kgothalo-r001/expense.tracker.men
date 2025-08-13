using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Repositories;

public class EfTransactionRepositoryTests
{
    //[Fact]
    //public async Task CreateAsync_AddsTransactionAndReturnsIt()
    //{
    //    var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
    //        .UseInMemoryDatabase(databaseName: "CreateTransactionTest")
    //        .Options;
    //    using var context = new ExpenseTrackerDbContext(options);
    //    var repo = new EfTransactionRepository(context);
    //    var transactionId = Guid.NewGuid().ToString();
    //    var userId = Guid.NewGuid().ToString();

    //    var transaction = new Transaction { Id = transactionId, Amount = 100, Type = TransactionType.EXPENSE, UserId = userId };
    //    var result = await repo.CreateAsync(transaction);
    //    result.Should().NotBeNull();
    //    result.Amount.Should().Be(100);
    //    context.Transactions.Count().Should().Be(1);
    //}

    //[Fact]
    //public async Task GetByIdAsync_ReturnsTransaction()
    //{
    //    var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
    //        .UseInMemoryDatabase(databaseName: "GetTransactionTest")
    //        .Options;
    //    using var context = new ExpenseTrackerDbContext(options);
    //    var repo = new EfTransactionRepository(context);
    //    var transactionId = Guid.NewGuid().ToString();
    //    var userId = Guid.NewGuid().ToString();

    //    var transaction = new Transaction { Id = transactionId, Amount = 200, Type = TransactionType.INCOME, UserId = userId };
    //    context.Transactions.Add(transaction);
    //    await context.SaveChangesAsync();
    //    var result = await repo.GetByIdAsync(transactionId);
    //    result.Should().NotBeNull();
    //    result!.Amount.Should().Be(200);
    //}

    //[Fact]
    //public async Task DeleteAsync_RemovesTransaction()
    //{
    //    var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
    //        .UseInMemoryDatabase(databaseName: "DeleteTransactionTest")
    //        .Options;
    //    using var context = new ExpenseTrackerDbContext(options);
    //    var repo = new EfTransactionRepository(context);
    //    var transactionId = Guid.NewGuid().ToString();
    //    var userId = Guid.NewGuid().ToString();

    //    var transaction = new Transaction { Id = transactionId, Amount = 300, Type = TransactionType.EXPENSE, UserId = userId };
    //    context.Transactions.Add(transaction);
    //    await context.SaveChangesAsync();
    //    var result = await repo.DeleteAsync(transactionId);
    //    result.Should().BeTrue();
    //    context.Transactions.Count().Should().Be(0);
    //}
}
