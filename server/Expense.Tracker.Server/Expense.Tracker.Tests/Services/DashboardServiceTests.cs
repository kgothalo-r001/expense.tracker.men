using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Implementation;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class DashboardServiceTests
{
    private readonly DashboardService _dashboardService;
    private readonly Mock<ITransactionRepository> _mockTransactionRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<IAnalyticsService> _mockAnalyticsService;
    private readonly Requestor _requestor;

    public DashboardServiceTests()
    {
        _mockTransactionRepo = new Mock<ITransactionRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockAnalyticsService = new Mock<IAnalyticsService>();
        _requestor = new Requestor { UserId = Guid.NewGuid().ToString() };
        _dashboardService = new DashboardService(_mockTransactionRepo.Object, _mockCategoryRepo.Object, _mockAnalyticsService.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithoutDateRange_ReturnsValidSummary()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, Type = TransactionType.INCOME, Amount = 1000 },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, Type = TransactionType.EXPENSE, Amount = 500 }
        };
        var categories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food", UserId = _requestor.UserId }
        };
        var recentTransactions = transactions;
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(transactions);
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(categories);
        _mockTransactionRepo.Setup(r => r.GetRecentByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(recentTransactions);

        var result = await _dashboardService.GetDashboardSummaryAsync(_requestor);
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1000);
        result.TotalExpenses.Should().Be(500);
        result.NetAmount.Should().Be(500);
        result.TransactionCount.Should().Be(2);
        result.RecentTransactions.Should().NotBeEmpty();
        result.RecentTransactions.Should().OnlyContain(t => t.UserId == _requestor.UserId);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithDateRange_ReturnsFilteredSummary()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "tx1", UserId = _requestor.UserId, Type = TransactionType.INCOME, Amount = 1000, Date = startDate.AddDays(1) },
            new Transaction { Id = "tx2", UserId = _requestor.UserId, Type = TransactionType.EXPENSE, Amount = 500, Date = endDate.AddDays(-1) }
        };
        var categories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food", UserId = _requestor.UserId }
        };
        var recentTransactions = transactions;
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndDateRangeAsync(It.IsAny<Guid>(), startDate, endDate)).ReturnsAsync(transactions);
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(categories);
        _mockTransactionRepo.Setup(r => r.GetRecentByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(recentTransactions);

        var result = await _dashboardService.GetDashboardSummaryAsync(_requestor, startDate, endDate);
        result.Should().NotBeNull();
        result.TotalIncome.Should().BeGreaterOrEqualTo(0);
        result.TotalExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecentTransactions.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate && t.UserId == _requestor.UserId);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WhenNoTransactions_ReturnsZeroValues()
    {
        _mockTransactionRepo.Setup(r => r.GetByUserIdAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<Transaction>());
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Category>());
        _mockTransactionRepo.Setup(r => r.GetRecentByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

        var result = await _dashboardService.GetDashboardSummaryAsync(_requestor);
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(0);
        result.TotalExpenses.Should().Be(0);
        result.NetAmount.Should().Be(0);
        result.TransactionCount.Should().Be(0);
        result.RecentTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WithDefaultPeriod_ReturnsValidAnalytics()
    {
        var monthlyAverage = 100m;
        var yearlyProjection = 1200m;
        var monthlyTrends = new List<MonthlySpending> { new MonthlySpending { Month = "2025-08", Amount = 100, TransactionCount = 1 } };
        var categoryTrends = new List<CategoryTrend> { new CategoryTrend { CategoryId = "cat1", CategoryName = "Food", CurrentMonthAmount = 100, PreviousMonthAmount = 80, PercentageChange = 25 } };
        _mockAnalyticsService.Setup(s => s.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, 12)).ReturnsAsync(monthlyAverage);
        _mockAnalyticsService.Setup(s => s.CalculateYearlyProjectionAsync(TransactionType.EXPENSE)).ReturnsAsync(yearlyProjection);
        _mockAnalyticsService.Setup(s => s.GetMonthlySpendingTrendsAsync(_requestor, 12)).ReturnsAsync(monthlyTrends);
        _mockAnalyticsService.Setup(s => s.GetCategoryTrendsAsync(_requestor)).ReturnsAsync(categoryTrends);

        var result = await _dashboardService.GetExpenseAnalyticsAsync(_requestor);
        result.Should().NotBeNull();
        result.MonthlySpendingTrends.Should().NotBeNull();
        result.CategoryTrends.Should().NotBeNull();
        result.YearlyProjection.Should().BeGreaterThan(0);
        result.MonthlyAverage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WithCustomPeriod_ReturnsAnalyticsForPeriod()
    {
        var monthsBack = 6;
        var monthlyAverage = 200m;
        var yearlyProjection = 2400m;
        var monthlyTrends = new List<MonthlySpending> { new MonthlySpending { Month = "2025-08", Amount = 200, TransactionCount = 2 } };
        var categoryTrends = new List<CategoryTrend> { new CategoryTrend { CategoryId = "cat1", CategoryName = "Food", CurrentMonthAmount = 200, PreviousMonthAmount = 100, PercentageChange = 100 } };
        _mockAnalyticsService.Setup(s => s.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, monthsBack)).ReturnsAsync(monthlyAverage);
        _mockAnalyticsService.Setup(s => s.CalculateYearlyProjectionAsync(TransactionType.EXPENSE)).ReturnsAsync(yearlyProjection);
        _mockAnalyticsService.Setup(s => s.GetMonthlySpendingTrendsAsync(_requestor, monthsBack)).ReturnsAsync(monthlyTrends);
        _mockAnalyticsService.Setup(s => s.GetCategoryTrendsAsync(_requestor)).ReturnsAsync(categoryTrends);

        var result = await _dashboardService.GetExpenseAnalyticsAsync(_requestor, monthsBack);
        result.Should().NotBeNull();
        result.MonthlySpendingTrends.Should().NotBeNull();
        result.CategoryTrends.Should().NotBeNull();
        result.YearlyProjection.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WhenNoExpenses_ReturnsZeroAnalytics()
    {
        _mockAnalyticsService.Setup(s => s.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, 12)).ReturnsAsync(0m);
        _mockAnalyticsService.Setup(s => s.CalculateYearlyProjectionAsync(TransactionType.EXPENSE)).ReturnsAsync(0m);
        _mockAnalyticsService.Setup(s => s.GetMonthlySpendingTrendsAsync(_requestor, 12)).ReturnsAsync(new List<MonthlySpending>());
        _mockAnalyticsService.Setup(s => s.GetCategoryTrendsAsync(_requestor)).ReturnsAsync(new List<CategoryTrend>());

        var result = await _dashboardService.GetExpenseAnalyticsAsync(_requestor);
        result.Should().NotBeNull();
        result.YearlyProjection.Should().Be(0);
        result.MonthlyAverage.Should().Be(0);
        result.CategoryTrends.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBudgetProjectionAsync_ReturnsValidProjection()
    {
        var projection = new BudgetProjection
        {
            ProjectedMonthlyExpenses = 300,
            ProjectedYearlyExpenses = 3600,
            RecommendedMonthlySavings = 150,
            CategoryProjections = new List<CategoryProjection>()
        };
        _mockAnalyticsService.Setup(s => s.GenerateBudgetProjectionAsync(_requestor)).ReturnsAsync(projection);

        var result = await _dashboardService.GetBudgetProjectionAsync(_requestor);
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.ProjectedYearlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecommendedMonthlySavings.Should().BeGreaterOrEqualTo(0);
        result.CategoryProjections.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBudgetProjectionAsync_WhenNoData_ReturnsZeroProjection()
    {
        var projection = new BudgetProjection
        {
            ProjectedMonthlyExpenses = 0,
            ProjectedYearlyExpenses = 0,
            RecommendedMonthlySavings = 0,
            CategoryProjections = new List<CategoryProjection>()
        };
        _mockAnalyticsService.Setup(s => s.GenerateBudgetProjectionAsync(_requestor)).ReturnsAsync(projection);

        var result = await _dashboardService.GetBudgetProjectionAsync(_requestor);
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().Be(0);
        result.ProjectedYearlyExpenses.Should().Be(0);
        result.RecommendedMonthlySavings.Should().Be(0);
        result.CategoryProjections.Should().BeEmpty();
    }
}
