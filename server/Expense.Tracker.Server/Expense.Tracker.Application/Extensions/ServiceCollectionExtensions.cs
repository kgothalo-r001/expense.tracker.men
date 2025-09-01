using Expense.Tracker.Peer.Helpers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Factories;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Implementation.Factories;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Validators;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for registering application services and dependencies
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {

        #region Public Extension Methods
        
        /// <summary>
        /// Add all expense tracker services and repositories to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerServices(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            // Configure Entity Framework DbContext
            ConfigureDatabase(services, connectionString);
            
            // Register all service dependencies
            RegisterRepositories(services);
            RegisterBusinessServices(services);
            RegisterAuthenticationServices(services);
            RegisterUtilityServices(services);
            RegisterFactoryServices(services);
            RegisterValidationServices(services);

            return services;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Configure Entity Framework DbContext with PostgreSQL
        /// </summary>
        private static void ConfigureDatabase(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ExpenseTrackerDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// Register all repository services
        /// </summary>
        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, EfCategoryRepository>();
            services.AddScoped<ITransactionRepository, EfTransactionRepository>();
            services.AddScoped<ITagRepository, EfTagRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();
        }

        /// <summary>
        /// Register all business logic services
        /// </summary>
        private static void RegisterBusinessServices(IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
        }

        /// <summary>
        /// Register all authentication-related services
        /// </summary>
        private static void RegisterAuthenticationServices(IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IUserValidationService, UserValidationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
        }

        /// <summary>
        /// Register utility and helper services
        /// </summary>
        private static void RegisterUtilityServices(IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITelemetryHelper, TelemetryHelper>();
        }

        /// <summary>
        /// Register factory services and strategies
        /// </summary>
        private static void RegisterFactoryServices(IServiceCollection services)
        {
            services.AddScoped<ICalculationStrategyFactory, CalculationStrategyFactory>();
            services.AddScoped<ITransactionFactory, TransactionFactory>();
        }

        /// <summary>
        /// Register validation services
        /// </summary>
        private static void RegisterValidationServices(IServiceCollection services)
        {
            services.AddScoped<ITransactionValidationService, TransactionValidator>();
        }

        #endregion
    }
}
