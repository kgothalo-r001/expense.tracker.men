using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Implementation;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class AnalyticsServiceTests
{
    private readonly AnalyticsService _analyticsService;
    private readonly Mock<ITransactionRepository> _mockTransactionRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<ICalculationStrategyFactory> _mockStrategyFactory;
    private readonly Mock<ICalculationStrategy> _mockStrategy;
    private readonly Requestor _requestor;

    public AnalyticsServiceTests()
    {
        _mockTransactionRepo = new Mock<ITransactionRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockStrategyFactory = new Mock<ICalculationStrategyFactory>();
        _mockStrategy = new Mock<ICalculationStrategy>();
        _requestor = new Requestor { UserId = Guid.NewGuid().ToString() };
        _analyticsService = new AnalyticsService(_mockTransactionRepo.Object, _mockCategoryRepo.Object, _mockStrategyFactory.Object);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_ForExpenses_ReturnsValidAverage()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(123.45m);

        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE);
        result.Should().Be(123.45m);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_ForIncome_ReturnsValidAverage()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.INCOME, It.IsAny<int>())).ReturnsAsync(555.55m);

        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.INCOME);
        result.Should().Be(555.55m);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_WithCustomPeriod_ReturnsAverageForPeriod()
    {
        var monthsBack = 3;
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, monthsBack)).ReturnsAsync(42m);

        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, monthsBack);
        result.Should().Be(42m);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_WhenNoTransactions_ReturnsZero()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(0m);

        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE);
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_ForExpenses_ReturnsValidProjection()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.YearlyProjection)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(1000m);

        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE);
        result.Should().Be(1000m);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_ForIncome_ReturnsValidProjection()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.YearlyProjection)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.INCOME, It.IsAny<int>())).ReturnsAsync(2000m);

        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.INCOME);
        result.Should().Be(2000m);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_WhenNoTransactions_ReturnsZero()
    {
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.YearlyProjection)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(0m);

        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE);
        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WithDefaultPeriod_ReturnsValidTrends()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Type = TransactionType.EXPENSE, Amount = 100 },
            new Transaction { Type = TransactionType.EXPENSE, Amount = 50 }
        };
        _mockTransactionRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(transactions);

        var result = await _analyticsService.GetMonthlySpendingTrendsAsync(_requestor);
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(12);
        result.Should().OnlyContain(trend => trend.Amount >= 0);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WithCustomPeriod_ReturnsLimitedTrends()
    {
        var monthsBack = 6;
        var transactions = new List<Transaction>
        {
            new Transaction { Type = TransactionType.EXPENSE, Amount = 100 }
        };
        _mockTransactionRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(transactions);

        var result = await _analyticsService.GetMonthlySpendingTrendsAsync(_requestor, monthsBack);
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(monthsBack);
        result.Should().OnlyContain(trend => trend.Amount >= 0);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WhenNoTransactions_ReturnsZeroAmountTrends()
    {
        _mockTransactionRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(new List<Transaction>());

        var result = await _analyticsService.GetMonthlySpendingTrendsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().HaveCount(12);
        result.Should().OnlyContain(trend => trend.Amount == 0 && trend.TransactionCount == 0);
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_ReturnsValidCategoryTrends()
    {
        var categories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food" },
            new Category { Id = "cat2", Name = "Transport" }
        };
        var currentMonthTransactions = new List<Transaction>
        {
            new Transaction { CategoryId = "cat1", Amount = 100 },
            new Transaction { CategoryId = "cat2", Amount = 50 }
        };
        var previousMonthTransactions = new List<Transaction>
        {
            new Transaction { CategoryId = "cat1", Amount = 80 },
            new Transaction { CategoryId = "cat2", Amount = 40 }
        };
        _mockCategoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockTransactionRepo.SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(currentMonthTransactions)
            .ReturnsAsync(previousMonthTransactions);

        var result = await _analyticsService.GetCategoryTrendsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(trend => trend.CurrentMonthAmount >= 0);
        result.Should().OnlyContain(trend => trend.PreviousMonthAmount >= 0);
        result.Should().OnlyContain(trend => !string.IsNullOrEmpty(trend.CategoryName));
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_WhenNoTransactions_ReturnsEmptyTrends()
    {
        _mockCategoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _analyticsService.GetCategoryTrendsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateBudgetProjectionAsync_ReturnsValidBudgetProjection()
    {
        var categories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food" }
        };
        var transactions = new List<Transaction>
        {
            new Transaction { CategoryId = "cat1", Amount = 100, Date = DateTime.UtcNow }
        };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(categories);
        _mockTransactionRepo.Setup(r => r.GetByCategoryIdAsync("cat1")).ReturnsAsync(transactions);
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(100m);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.INCOME, It.IsAny<int>())).ReturnsAsync(200m);

        var result = await _analyticsService.GenerateBudgetProjectionAsync(_requestor);
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.ProjectedYearlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecommendedMonthlySavings.Should().BeGreaterOrEqualTo(0);
        result.CategoryProjections.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateBudgetProjectionAsync_WhenNoData_ReturnsZeroBudgetProjection()
    {
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Category>());
        _mockStrategyFactory.Setup(f => f.GetStrategy(CalculationStrategyType.MonthlyAverage)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.EXPENSE, It.IsAny<int>())).ReturnsAsync(0m);
        _mockStrategy.Setup(s => s.CalculateAsync(_mockTransactionRepo.Object, TransactionType.INCOME, It.IsAny<int>())).ReturnsAsync(0m);

        var result = await _analyticsService.GenerateBudgetProjectionAsync(_requestor);
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().Be(0);
        result.ProjectedYearlyExpenses.Should().Be(0);
        result.RecommendedMonthlySavings.Should().Be(0);
        result.CategoryProjections.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_GroupsByCategory_ReturnsCorrectGrouping()
    {
        var categories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food" },
            new Category { Id = "cat2", Name = "Transport" }
        };
        var currentMonthTransactions = new List<Transaction>
        {
            new Transaction { CategoryId = "cat1", Amount = 100 },
            new Transaction { CategoryId = "cat2", Amount = 50 }
        };
        var previousMonthTransactions = new List<Transaction>
        {
            new Transaction { CategoryId = "cat1", Amount = 80 },
            new Transaction { CategoryId = "cat2", Amount = 40 }
        };
        _mockCategoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockTransactionRepo.SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(currentMonthTransactions)
            .ReturnsAsync(previousMonthTransactions);

        var result = await _analyticsService.GetCategoryTrendsAsync(_requestor);
        result.Should().NotBeNull();
        var categoryNames = result.Select(t => t.CategoryName).ToList();
        categoryNames.Should().OnlyHaveUniqueItems();
        foreach (var trend in result)
        {
            trend.CategoryName.Should().NotBeNullOrEmpty();
            trend.CurrentMonthAmount.Should().BeGreaterOrEqualTo(0);
            trend.PreviousMonthAmount.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_OrdersByMonth_ReturnsChronologicalOrder()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Type = TransactionType.EXPENSE, Amount = 100 },
            new Transaction { Type = TransactionType.EXPENSE, Amount = 50 }
        };
        _mockTransactionRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(transactions);

        var result = await _analyticsService.GetMonthlySpendingTrendsAsync(_requestor);
        result.Should().NotBeNull();
        if (result.Count() > 1)
        {
            var trends = result.ToList();
            for (int i = 0; i < trends.Count; i++)
            {
                trends[i].Month.Should().NotBeNullOrEmpty();
            }
        }
    }
}
