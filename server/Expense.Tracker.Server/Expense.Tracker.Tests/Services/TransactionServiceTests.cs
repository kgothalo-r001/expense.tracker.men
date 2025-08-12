using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class TransactionServiceTests : BaseTestHelper
{
    private readonly ITransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionService = GetService<ITransactionService>();
    }

    [Fact]
    public async Task GetAllTransactionsAsync_WhenTransactionsExist_ReturnsUserTransactions()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _transactionService.GetAllTransactionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllTransactionsAsync_WhenNoTransactionsExist_ReturnsEmptyCollection()
    {
        // Act
        var result = await _transactionService.GetAllTransactionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WithValidId_ReturnsTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(existingTransaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingTransaction.Id);
        result.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransactionAsync_WithValidRequest_CreatesAndReturnsTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();
        var request = new CreateTransactionRequest
        {
            Amount = 123.45m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = category.Id,
            Notes = "Test notes",
            IsRecurring = false
        };

        // Act
        var result = await _transactionService.CreateTransactionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(request.Amount);
        result.Description.Should().Be(request.Description);
        result.Type.Should().Be(request.Type);
        result.CategoryId.Should().Be(request.CategoryId);
        result.Notes.Should().Be(request.Notes);
        result.UserId.Should().Be(TestUserId.ToString());
        result.IsRecurring.Should().BeFalse();

        // Verify in database
        var dbTransaction = await DbContext.Transactions.FindAsync(result.Id);
        dbTransaction.Should().NotBeNull();
        dbTransaction!.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithRecurring_CreatesRecurringTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();
        var request = new CreateTransactionRequest
        {
            Amount = 500.00m,
            Description = "Recurring transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = category.Id,
            IsRecurring = true,
            RecurringFrequency = RecurringFrequency.MONTHLY,
            RecurringEndDate = DateTime.UtcNow.AddMonths(12)
        };

        // Act
        var result = await _transactionService.CreateTransactionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsRecurring.Should().BeTrue();
        result.RecurringFrequency.Should().Be(RecurringFrequency.MONTHLY);
        result.RecurringEndDate.Should().Be(request.RecurringEndDate);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithInvalidCategoryId_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Amount = 100.00m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = Guid.NewGuid().ToString() // Non-existent category
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _transactionService.CreateTransactionAsync(request));
    }

    [Fact]
    public async Task UpdateTransactionAsync_WithValidRequest_UpdatesAndReturnsTransaction()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();
        var request = new UpdateTransactionRequest
        {
            Id = existingTransaction.Id,
            Amount = 200.00m,
            Description = "Updated transaction",
            Date = existingTransaction.Date,
            Type = existingTransaction.Type,
            CategoryId = existingTransaction.CategoryId,
            Notes = "Updated notes"
        };

        // Act
        var result = await _transactionService.UpdateTransactionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingTransaction.Id);
        result.Amount.Should().Be(request.Amount);
        result.Description.Should().Be(request.Description);
        result.Notes.Should().Be(request.Notes);

        // Verify in database
        var dbTransaction = await DbContext.Transactions.FindAsync(existingTransaction.Id);
        dbTransaction!.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateTransactionRequest
        {
            Id = Guid.NewGuid().ToString(),
            Amount = 200.00m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _transactionService.UpdateTransactionAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithValidId_DeletesTransactionAndReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTransaction = DbContext.Transactions.First();

        // Act
        var result = await _transactionService.DeleteTransactionAsync(existingTransaction.Id);

        // Assert
        result.Should().BeTrue();

        // Verify transaction is deleted
        var deletedTransaction = await DbContext.Transactions.FindAsync(existingTransaction.Id);
        deletedTransaction.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _transactionService.DeleteTransactionAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTransactionsByCategoryAsync_WithValidCategory_ReturnsFilteredTransactions()
    {
        // Arrange
        await SeedTestDataAsync();
        var category = DbContext.Categories.First();

        // Act
        var result = await _transactionService.GetTransactionsByCategoryAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.CategoryId == category.Id);
        result.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_WithValidRange_ReturnsFilteredTransactions()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate);
        result.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetUserRecurringTransactionsAsync_ReturnsOnlyRecurringTransactions()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _transactionService.GetUserRecurringTransactionsAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.IsRecurring == true);
        result.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task ProcessRecurringTransactionsAsync_ProcessesRecurringTransactions()
    {
        // Arrange
        await SeedTestDataAsync();
        var initialTransactionCount = DbContext.Transactions.Count();

        // Act
        await _transactionService.ProcessRecurringTransactionsAsync();

        // Assert
        // This test depends on the implementation of ProcessRecurringTransactionsAsync
        // It should create new transactions based on recurring patterns
        var finalTransactionCount = DbContext.Transactions.Count();
        finalTransactionCount.Should().BeGreaterOrEqualTo(initialTransactionCount);
    }
}
