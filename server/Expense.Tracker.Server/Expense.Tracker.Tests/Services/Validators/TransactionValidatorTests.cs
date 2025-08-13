using Xunit;
using FluentAssertions;
using Moq;
using Expense.Tracker.Services.Validators;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Exceptions;

namespace Expense.Tracker.Tests.Services.Validators;

public class TransactionValidatorTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly TransactionValidator _validator;
    private readonly Guid _userId;

    public TransactionValidatorTests()
    {
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _validator = new TransactionValidator(_mockCategoryRepo.Object);
        _userId = Guid.NewGuid();
    }

    [Fact]
    public async Task ValidateCategoryExistsAsync_WhenCategoryExists_DoesNotThrow()
    {
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(_userId, "cat1")).ReturnsAsync(new Category { Id = "cat1" });
        var act = async () => await _validator.ValidateCategoryExistsAsync(_userId, "cat1");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateCategoryExistsAsync_WhenCategoryMissing_ThrowsCategoryNotFoundException()
    {
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(_userId, "cat1")).ReturnsAsync((Category?)null);
        var act = async () => await _validator.ValidateCategoryExistsAsync(_userId, "cat1");
        await act.Should().ThrowAsync<CategoryNotFoundException>();
    }

    [Fact]
    public void ValidateRecurringTransactionSettings_NotRecurring_DoesNotThrow()
    {
        var act = () => _validator.ValidateRecurringTransactionSettings(false, null, null, DateTime.UtcNow);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateRecurringTransactionSettings_RecurringNoFrequency_ThrowsInvalidRecurringTransactionException()
    {
        var act = () => _validator.ValidateRecurringTransactionSettings(true, null, null, DateTime.UtcNow);
        act.Should().Throw<InvalidRecurringTransactionException>().WithMessage("*frequency*");
    }

    [Fact]
    public void ValidateRecurringTransactionSettings_RecurringEndDateBeforeTransactionDate_ThrowsInvalidRecurringTransactionException()
    {
        var transactionDate = DateTime.UtcNow;
        var endDate = transactionDate.AddDays(-1);
        var act = () => _validator.ValidateRecurringTransactionSettings(true, RecurringFrequency.MONTHLY, endDate, transactionDate);
        act.Should().Throw<InvalidRecurringTransactionException>().WithMessage("*end date*");
    }

    [Fact]
    public void ValidateRecurringTransactionSettings_ValidRecurring_DoesNotThrow()
    {
        var transactionDate = DateTime.UtcNow;
        var endDate = transactionDate.AddDays(10);
        var act = () => _validator.ValidateRecurringTransactionSettings(true, RecurringFrequency.MONTHLY, endDate, transactionDate);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateRecurringTransactionSettings_WithRequestObject_Valid_DoesNotThrow()
    {
        var request = new CreateTransactionRequest
        {
            IsRecurring = true,
            RecurringFrequency = RecurringFrequency.MONTHLY,
            RecurringEndDate = DateTime.UtcNow.AddDays(10),
            Date = DateTime.UtcNow
        };
        var act = () => _validator.ValidateRecurringTransactionSettings(request);
        act.Should().NotThrow();
    }
}
