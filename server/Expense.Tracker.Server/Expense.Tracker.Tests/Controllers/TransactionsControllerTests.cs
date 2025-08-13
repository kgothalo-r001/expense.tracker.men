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
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Mock<ILogger<TransactionsController>> _mockLogger;

    public TransactionsControllerTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _mockLogger = new Mock<ILogger<TransactionsController>>();
        _controller = new TransactionsController(_mockTransactionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTransactions_WhenTransactionsExist_ReturnsOkWithTransactions()
    {
        var expectedTransactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = TestUserId.ToString(), Amount = 100, CategoryId = "cat1", Date = DateTime.UtcNow },
            new Transaction { Id = "tx2", UserId = TestUserId.ToString(), Amount = 200, CategoryId = "cat2", Date = DateTime.UtcNow }
        };
        _mockTransactionService.Setup(s => s.GetAllTransactionsAsync(It.IsAny<Requestor>())).ReturnsAsync(expectedTransactions);

        var result = await _controller.GetTransactions();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().HaveCountGreaterThan(0);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactions_WithCategoryFilter_ReturnsFilteredTransactions()
    {
        var categoryId = "cat1";
        var expectedTransactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = TestUserId.ToString(), Amount = 100, CategoryId = categoryId, Date = DateTime.UtcNow }
        };
        _mockTransactionService.Setup(s => s.GetTransactionsByCategoryAsync(categoryId, It.IsAny<Requestor>())).ReturnsAsync(expectedTransactions);

        var result = await _controller.GetTransactions(categoryId: categoryId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.CategoryId == categoryId);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransactions_WithDateRange_ReturnsFilteredTransactions()
    {
        var startDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow;
        var expectedTransactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = TestUserId.ToString(), Amount = 100, CategoryId = "cat1", Date = startDate.AddHours(1) },
            new Transaction { Id = "tx2", UserId = TestUserId.ToString(), Amount = 200, CategoryId = "cat2", Date = endDate.AddHours(-1) }
        };
        _mockTransactionService.Setup(s => s.GetTransactionsByDateRangeAsync(startDate, endDate, It.IsAny<Requestor>())).ReturnsAsync(expectedTransactions);

        var result = await _controller.GetTransactions(startDate: startDate, endDate: endDate);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate);
    }

    [Fact]
    public async Task GetTransaction_WithValidId_ReturnsOkWithTransaction()
    {
        var transactionId = "tx1";
        var expectedTransaction = new Transaction { Id = transactionId, UserId = TestUserId.ToString(), Amount = 100, CategoryId = "cat1", Date = DateTime.UtcNow };
        _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(transactionId, It.IsAny<Requestor>())).ReturnsAsync(expectedTransaction);

        var result = await _controller.GetTransaction(transactionId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transaction = okResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Id.Should().Be(transactionId);
        transaction.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetTransaction_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "tx999";
        _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync((Transaction?)null);

        var result = await _controller.GetTransaction(nonExistentId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateTransaction_WithValidRequest_ReturnsCreatedWithTransaction()
    {
        var request = new CreateTransactionRequest
        {
            Amount = 100.50m,
            Description = "Test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1",
            Notes = "Test notes",
            IsRecurring = false
        };
        var createdTransaction = new Transaction
        {
            Id = "tx1",
            Amount = request.Amount,
            Description = request.Description,
            Date = request.Date,
            Type = request.Type,
            CategoryId = request.CategoryId,
            Notes = request.Notes,
            IsRecurring = request.IsRecurring,
            UserId = TestUserId.ToString()
        };
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request, It.IsAny<Requestor>())).ReturnsAsync(createdTransaction);

        var result = await _controller.CreateTransaction(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var transaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Amount.Should().Be(request.Amount);
        transaction.Description.Should().Be(request.Description);
        transaction.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task CreateTransaction_WithRecurring_ReturnsCreatedWithRecurringTransaction()
    {
        var request = new CreateTransactionRequest
        {
            Amount = 200.00m,
            Description = "Recurring test transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1",
            IsRecurring = true,
            RecurringFrequency = RecurringFrequency.MONTHLY,
            RecurringEndDate = DateTime.UtcNow.AddMonths(12)
        };
        var createdTransaction = new Transaction
        {
            Id = "tx2",
            Amount = request.Amount,
            Description = request.Description,
            Date = request.Date,
            Type = request.Type,
            CategoryId = request.CategoryId,
            IsRecurring = request.IsRecurring,
            RecurringFrequency = request.RecurringFrequency,
            RecurringEndDate = request.RecurringEndDate,
            UserId = TestUserId.ToString()
        };
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request, It.IsAny<Requestor>())).ReturnsAsync(createdTransaction);

        var result = await _controller.CreateTransaction(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var transaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.IsRecurring.Should().BeTrue();
        transaction.RecurringFrequency.Should().Be(RecurringFrequency.MONTHLY);
        transaction.RecurringEndDate.Should().Be(request.RecurringEndDate);
    }

    [Fact]
    public async Task UpdateTransaction_WithValidRequest_ReturnsOkWithUpdatedTransaction()
    {
        var request = new UpdateTransactionRequest
        {
            Id = "tx1",
            Amount = 150.75m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat1",
            Notes = "Updated notes"
        };
        var updatedTransaction = new Transaction
        {
            Id = request.Id,
            Amount = (decimal)request.Amount,
            Description = request.Description,
            Date = (DateTime)request.Date,
            Type = (TransactionType)request.Type,
            CategoryId = request.CategoryId,
            Notes = request.Notes,
            UserId = TestUserId.ToString()
        };
        _mockTransactionService.Setup(s => s.UpdateTransactionAsync(request, It.IsAny<Requestor>())).ReturnsAsync(updatedTransaction);

        var result = await _controller.UpdateTransaction(request.Id, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transaction = okResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Amount.Should().Be(request.Amount);
        transaction.Description.Should().Be(request.Description);
        transaction.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task UpdateTransaction_WithInvalidId_ReturnsNotFound()
    {
        var request = new UpdateTransactionRequest
        {
            Id = "tx999",
            Amount = 150.75m,
            Description = "Updated transaction",
            Date = DateTime.UtcNow,
            Type = TransactionType.EXPENSE,
            CategoryId = "cat999"
        };
        _mockTransactionService.Setup(s => s.UpdateTransactionAsync(request, It.IsAny<Requestor>())).ReturnsAsync((Transaction?)null);

        var result = await _controller.UpdateTransaction(request.Id, request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTransaction_WithValidId_ReturnsNoContent()
    {
        var transactionId = "tx1";
        _mockTransactionService.Setup(s => s.DeleteTransactionAsync(transactionId, It.IsAny<Requestor>())).ReturnsAsync(true);

        var result = await _controller.DeleteTransaction(transactionId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTransaction_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "tx999";
        _mockTransactionService.Setup(s => s.DeleteTransactionAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync(false);

        var result = await _controller.DeleteTransaction(nonExistentId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRecurringTransactions_ReturnsOnlyRecurringTransactions()
    {
        var expectedTransactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = TestUserId.ToString(), IsRecurring = true },
            new Transaction { Id = "tx2", UserId = TestUserId.ToString(), IsRecurring = true }
        };
        _mockTransactionService.Setup(s => s.GetRecurringTransactionsAsync(It.IsAny<Requestor>())).ReturnsAsync(expectedTransactions);

        var result = await _controller.GetRecurringTransactions();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<Transaction>>().Subject;
        transactions.Should().OnlyContain(t => t.IsRecurring == true);
        transactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }
}
