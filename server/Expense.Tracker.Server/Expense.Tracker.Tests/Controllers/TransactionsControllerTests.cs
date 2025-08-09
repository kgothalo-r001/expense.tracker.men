using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class TransactionsControllerTests : BaseTestHelper
{
    private readonly TransactionsController _controller;
    private readonly ITransactionService _transactionService;
    private readonly Mock<ILogger<TransactionsController>> _mockLogger;

    public TransactionsControllerTests()
    {
        _transactionService = GetService<ITransactionService>();
        _mockLogger = new Mock<ILogger<TransactionsController>>();
        _controller = new TransactionsController(_transactionService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTransactions_WhenTransactionsExist_ReturnsOkWithTransactions()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetTransactions();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().HaveCountGreaterThan(0);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactions_WithCategoryFilter_ReturnsFilteredTransactions()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();

        // Act
        var result = await _controller.GetTransactions(categoryId: category.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.CategoryId == category.Id);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactions_WithDateRange_ReturnsFilteredTransactions()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _controller.GetTransactions(startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate);
    }

    [Fact]
    public async Task GetTransaction_WithValidId_ReturnsOkWithTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();

        // Act
        var result = await _controller.GetTransaction(existingTransaction.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transaction = okResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Id.Should().Be(existingTransaction.Id);
        transaction.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransaction_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.GetTransaction(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateTransaction_WithValidRequest_ReturnsCreatedWithTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();
        var request = new CreateTransactionRequest
        {
            Amount = 100.50m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = category.Id,
            Notes = "Test notes",
            IsRecurring = false
        };

        // Act
        var result = await _controller.CreateTransaction(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var transaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Amount.Should().Be(request.Amount);
        transaction.Description.Should().Be(request.Description);
        transaction.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task CreateTransaction_WithRecurring_ReturnsCreatedWithRecurringTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();
        var request = new CreateTransactionRequest
        {
            Amount = 200.00m,
            Description = "Recurring test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = category.Id,
            IsRecurring = true,
            RecurringFrequency = RecurringFrequency.MONTHLY,
            RecurringEndDate = DateTime.UtcNow.AddMonths(12)
        };

        // Act
        var result = await _controller.CreateTransaction(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var transaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.IsRecurring.Should().BeTrue();
        transaction.RecurringFrequency.Should().Be(RecurringFrequency.MONTHLY);
        transaction.RecurringEndDate.Should().Be(request.RecurringEndDate);
    }

    [Fact]
    public async Task UpdateTransaction_WithValidRequest_ReturnsOkWithUpdatedTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();
        var request = new UpdateTransactionRequest
        {
            Id = existingTransaction.Id,
            Amount = 150.75m,
            Description = "Updated transaction",
            Date = existingTransaction.Date,
            Type = existingTransaction.Type,
            CategoryId = existingTransaction.CategoryId,
            Notes = "Updated notes"
        };

        // Act
        var result = await _controller.UpdateTransaction(existingTransaction.Id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transaction = okResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Amount.Should().Be(request.Amount);
        transaction.Description.Should().Be(request.Description);
        transaction.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task UpdateTransaction_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateTransactionRequest
        {
            Id = Guid.NewGuid().ToString(),
            Amount = 150.75m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _controller.UpdateTransaction(request.Id, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteTransaction_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();

        // Act
        var result = await _controller.DeleteTransaction(existingTransaction.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify transaction is deleted
        var deletedTransaction = await DbContext.Transactions.FindAsync(existingTransaction.Id);
        deletedTransaction.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTransaction_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.DeleteTransaction(nonExistentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRecurringTransactions_ReturnsOnlyRecurringTransactions()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetRecurringTransactions();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.IsRecurring == true);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }
}
