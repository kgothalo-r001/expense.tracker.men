using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Implementation;
using Microsoft.Extensions.Logging;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class TransactionServiceTests
{
    private readonly TransactionService _transactionService;
    private readonly Mock<ITransactionRepository> _mockTransactionRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<ITagRepository> _mockTagRepo;
    private readonly Mock<ITransactionValidationService> _mockValidator;
    private readonly Mock<ITransactionFactory> _mockFactory;
    private readonly Mock<ILogger<TransactionService>> _mockLogger;
    private readonly Requestor _requestor;

    public TransactionServiceTests()
    {
        _mockTransactionRepo = new Mock<ITransactionRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockTagRepo = new Mock<ITagRepository>();
        _mockValidator = new Mock<ITransactionValidationService>();
        _mockFactory = new Mock<ITransactionFactory>();
        _mockLogger = new Mock<ILogger<TransactionService>>();
        _requestor = new Requestor { UserId = Guid.NewGuid().ToString() };
        _transactionService = new TransactionService(
            _mockTransactionRepo.Object,
            _mockCategoryRepo.Object,
            _mockTagRepo.Object,
            _mockValidator.Object,
            _mockFactory.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllTransactionsAsync_WhenTransactionsExist_ReturnsUserTransactions()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, Amount = 100, Type = TransactionType.EXPENSE },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, Amount = 200, Type = TransactionType.INCOME }
        };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(transactions);
        var result = await _transactionService.GetAllTransactionsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.UserId == _requestor.UserId);
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllTransactionsAsync_WhenNoTransactionsExist_ReturnsEmptyCollection()
    {
        _mockTransactionRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Transaction>());
        var result = await _transactionService.GetAllTransactionsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WithValidId_ReturnsTransaction()
    {
        var transaction = new Transaction { Id = "tx1", UserId = _requestor.UserId, Amount = 100 };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), transaction.Id)).ReturnsAsync(transaction);
        var result = await _transactionService.GetTransactionByIdAsync(transaction.Id, _requestor);
        result.Should().NotBeNull();
        result!.Id.Should().Be(transaction.Id);
        result.UserId.Should().Be(_requestor.UserId);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WithInvalidId_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), nonExistentId)).ReturnsAsync((Transaction?)null);
        var result = await _transactionService.GetTransactionByIdAsync(nonExistentId, _requestor);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransactionAsync_WithValidRequest_CreatesAndReturnsTransaction()
    {
        var request = new CreateTransactionRequest
        {
            Amount = 123.45m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1",
            Notes = "Test notes",
            IsRecurring = false
        };
        _mockValidator.Setup(v => v.ValidateCategoryExistsAsync(It.IsAny<Guid>(), request.CategoryId)).Returns(Task.CompletedTask);
        _mockValidator.Setup(v => v.ValidateRecurringTransactionSettings(request));
        var transaction = new Transaction
        {
            Id = "tx1",
            UserId = _requestor.UserId,
            Amount = request.Amount,
            Description = request.Description,
            Type = request.Type,
            CategoryId = request.CategoryId,
            Notes = request.Notes,
            IsRecurring = false
        };
        _mockFactory.Setup(f => f.CreateTransaction(request, _requestor.UserId)).Returns(transaction);
        _mockTransactionRepo.Setup(r => r.CreateAsync(transaction)).ReturnsAsync(transaction);
        var result = await _transactionService.CreateTransactionAsync(request, _requestor);
        result.Should().NotBeNull();
        result.Amount.Should().Be(request.Amount);
        result.Description.Should().Be(request.Description);
        result.Type.Should().Be(request.Type);
        result.CategoryId.Should().Be(request.CategoryId);
        result.Notes.Should().Be(request.Notes);
        result.UserId.Should().Be(_requestor.UserId);
        result.IsRecurring.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTransactionAsync_WithRecurring_CreatesRecurringTransaction()
    {
        var request = new CreateTransactionRequest
        {
            Amount = 500.00m,
            Description = "Recurring transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1",
            IsRecurring = true,
            RecurringFrequency = RecurringFrequency.MONTHLY,
            RecurringEndDate = DateTime.UtcNow.AddMonths(12)
        };
        _mockValidator.Setup(v => v.ValidateCategoryExistsAsync(It.IsAny<Guid>(), request.CategoryId)).Returns(Task.CompletedTask);
        _mockValidator.Setup(v => v.ValidateRecurringTransactionSettings(request));
        var transaction = new Transaction
        {
            Id = "tx2",
            UserId = _requestor.UserId,
            Amount = request.Amount,
            Description = request.Description,
            Type = request.Type,
            CategoryId = request.CategoryId,
            IsRecurring = true,
            RecurringFrequency = request.RecurringFrequency,
            RecurringEndDate = request.RecurringEndDate
        };
        _mockFactory.Setup(f => f.CreateTransaction(request, _requestor.UserId)).Returns(transaction);
        _mockTransactionRepo.Setup(r => r.CreateAsync(transaction)).ReturnsAsync(transaction);
        var result = await _transactionService.CreateTransactionAsync(request, _requestor);
        result.Should().NotBeNull();
        result.IsRecurring.Should().BeTrue();
        result.RecurringFrequency.Should().Be(RecurringFrequency.MONTHLY);
        result.RecurringEndDate.Should().Be(request.RecurringEndDate);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithInvalidCategoryId_ThrowsCategoryNotFoundException()
    {
        var request = new CreateTransactionRequest
        {
            Amount = 100.00m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "invalid-cat"
        };
        _mockValidator.Setup(v => v.ValidateCategoryExistsAsync(It.IsAny<Guid>(), request.CategoryId)).ThrowsAsync(new Expense.Tracker.Services.Exceptions.CategoryNotFoundException("Category not found"));
        await Assert.ThrowsAsync<Expense.Tracker.Services.Exceptions.CategoryNotFoundException>(async () => await _transactionService.CreateTransactionAsync(request, _requestor));
    }

    [Fact]
    public async Task UpdateTransactionAsync_WithValidRequest_UpdatesAndReturnsTransaction()
    {
        var existingTransaction = new Transaction { Id = "tx1", UserId = _requestor.UserId, Amount = 100, Description = "Old", CategoryId = "cat1", Notes = "Old notes" };
        var request = new UpdateTransactionRequest
        {
            Id = existingTransaction.Id,
            Amount = 200.00m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = existingTransaction.CategoryId,
            Notes = "Updated notes"
        };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), request.Id)).ReturnsAsync(existingTransaction);
        _mockTransactionRepo.Setup(r => r.UpdateAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => t);
        var result = await _transactionService.UpdateTransactionAsync(request, _requestor);
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingTransaction.Id);
        result.Amount.Should().Be(request.Amount);
        result.Description.Should().Be(request.Description);
        result.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WithInvalidId_ReturnsNull()
    {
        var request = new UpdateTransactionRequest
        {
            Id = Guid.NewGuid().ToString(),
            Amount = 200.00m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1"
        };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), request.Id)).ReturnsAsync((Transaction?)null);
        var result = await _transactionService.UpdateTransactionAsync(request, _requestor);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithValidId_DeletesTransactionAndReturnsTrue()
    {
        var transaction = new Transaction { Id = "tx1", UserId = _requestor.UserId };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), transaction.Id)).ReturnsAsync(transaction);
        _mockTransactionRepo.Setup(r => r.DeleteAsync(transaction.Id)).ReturnsAsync(true);
        var result = await _transactionService.DeleteTransactionAsync(transaction.Id, _requestor);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithInvalidId_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), nonExistentId)).ReturnsAsync((Transaction?)null);
        var result = await _transactionService.DeleteTransactionAsync(nonExistentId, _requestor);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTransactionsByCategoryAsync_WithValidCategory_ReturnsFilteredTransactions()
    {
        var categoryId = "cat1";
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, CategoryId = categoryId },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, CategoryId = categoryId }
        };
        _mockValidator.Setup(v => v.ValidateCategoryExistsAsync(It.IsAny<Guid>(), categoryId)).Returns(Task.CompletedTask);
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndCategoryIdAsync(It.IsAny<Guid>(), categoryId)).ReturnsAsync(transactions);
        var result = await _transactionService.GetTransactionsByCategoryAsync(categoryId, _requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.CategoryId == categoryId);
        result.Should().OnlyContain(t => t.UserId == _requestor.UserId);
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_WithValidRange_ReturnsFilteredTransactions()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow.AddDays(1);
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, Date = startDate.AddDays(1) },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, Date = endDate.AddDays(-1) }
        };
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndDateRangeAsync(It.IsAny<Guid>(), startDate, endDate)).ReturnsAsync(transactions);
        var result = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate, _requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate);
        result.Should().OnlyContain(t => t.UserId == _requestor.UserId);
    }

    [Fact]
    public async Task GetRecurringTransactionsAsync_ReturnsOnlyRecurringTransactions()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, IsRecurring = true },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, IsRecurring = true }
        };
        _mockTransactionRepo.Setup(r => r.GetRecurringByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(transactions);
        var result = await _transactionService.GetRecurringTransactionsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.IsRecurring == true);
        result.Should().OnlyContain(t => t.UserId == _requestor.UserId);
    }
}
