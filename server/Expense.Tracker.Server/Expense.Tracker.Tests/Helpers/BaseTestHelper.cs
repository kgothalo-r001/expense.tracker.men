using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Helpers;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Moq;

namespace Expense.Tracker.Tests.Helpers;

public abstract class BaseTestHelper : IDisposable
{
    protected ServiceProvider ServiceProvider { get; private set; }
    protected ExpenseTrackerDbContext DbContext { get; private set; }
    protected Guid TestUserId { get; } = Guid.NewGuid();
    private readonly string _databaseName;

    protected BaseTestHelper()
    {
        _databaseName = $"ExpenseTracker_Test_{GetType().Name}_{Guid.NewGuid()}";
        ServiceProvider = CreateServiceProvider(_databaseName);
        DbContext = ServiceProvider.GetRequiredService<ExpenseTrackerDbContext>();
        
        DbContext.Database.EnsureCreated();
    }

    protected async Task ClearDatabaseAsync()
    {
        DbContext.UserSessions.RemoveRange(DbContext.UserSessions);
        DbContext.Transactions.RemoveRange(DbContext.Transactions);
        DbContext.Categories.RemoveRange(DbContext.Categories);
        DbContext.Tags.RemoveRange(DbContext.Tags);
        DbContext.Users.RemoveRange(DbContext.Users);
        
        await DbContext.SaveChangesAsync();
    }

    private ServiceProvider CreateServiceProvider(string databaseName)
    {
        var services = new ServiceCollection();

        // Add in-memory database
        services.AddDbContext<ExpenseTrackerDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        var testConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:Secret", "test-secret-key-for-jwt-token-generation-in-tests-12345"},
                {"Jwt:ExpiryMinutes", "60"}
            })
            .Build();
        services.AddSingleton<IConfiguration>(testConfig);

        // Add repositories
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<ITransactionRepository, EfTransactionRepository>();
        services.AddScoped<ITagRepository, EfTagRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();

        // Add helper services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddScoped<IAuthenticationService>(sp =>
        {
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var tokenService = sp.GetRequiredService<ITokenService>();
            var sessionService = sp.GetRequiredService<ISessionService>();
            var userValidationService = sp.GetRequiredService<IUserValidationService>();
            var logger = sp.GetRequiredService<ILogger<AuthenticationService>>();
            return new AuthenticationService(userRepo, tokenService, sessionService, userValidationService, logger);
        });

        // Mock HTTP context for current user service
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
        }, "test"));
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor.Object);
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services.BuildServiceProvider();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    protected async Task SeedTestDataAsync()
    {
        // Create test user
        var testUser = new User
        {
            Id = TestUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        DbContext.Users.Add(testUser);

        // Create test categories
        var categories = new[]
        {
            new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Food",
                Type = CategoryType.EXPENSE,
                Color = "#FF0000",
                Icon = "food",
                UserId = TestUserId.ToString(),
                IsDefault = false
            },
            new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Salary",
                Type = CategoryType.INCOME,
                Color = "#00FF00",
                Icon = "work",
                UserId = TestUserId.ToString(),
                IsDefault = false
            }
        };

        DbContext.Categories.AddRange(categories);

        // Create test tags
        var tags = new[]
        {
            new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = "urgent",
                UsageCount = 0
            },
            new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = "monthly",
                UsageCount = 0
            }
        };

        DbContext.Tags.AddRange(tags);

        // Create test transactions
        var transactions = new[]
        {
            new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 50.00m,
                Description = "Grocery shopping",
                Date = DateTime.UtcNow.AddDays(-1),
                Type = TransactionType.EXPENSE,
                CategoryId = categories[0].Id,
                UserId = TestUserId.ToString(),
                IsRecurring = false
            },
            new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 3000.00m,
                Description = "Monthly salary",
                Date = DateTime.UtcNow.AddDays(-30),
                Type = TransactionType.INCOME,
                CategoryId = categories[1].Id,
                UserId = TestUserId.ToString(),
                IsRecurring = true,
                RecurringFrequency = RecurringFrequency.MONTHLY
            }
        };

        DbContext.Transactions.AddRange(transactions);

        await DbContext.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
    }
}
