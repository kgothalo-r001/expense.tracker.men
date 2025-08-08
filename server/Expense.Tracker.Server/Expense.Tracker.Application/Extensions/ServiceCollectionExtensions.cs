using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Expense.Tracker.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add all expense tracker services and repositories to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerServices(this IServiceCollection services, string connectionString)
        {
            // Configure DbContext with PostgreSQL (using value converters for enums)
            services.AddDbContext<ExpenseTrackerDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // Register HTTP context accessor for current user service
            services.AddHttpContextAccessor();

            // Register repositories as scoped (Entity Framework)
            services.AddScoped<ICategoryRepository, EfCategoryRepository>();
            services.AddScoped<ITransactionRepository, EfTransactionRepository>();
            services.AddScoped<ITagRepository, EfTagRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();

            // Register utility services
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Register business services as scoped
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // Register authentication services
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }

        /// <summary>
        /// Add and configure controllers from the Peer assembly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerControllers(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddApplicationPart(typeof(Expense.Tracker.Peer.Controllers.CategoriesController).Assembly)
                .AddControllersAsServices()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization to use enum names instead of integers
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            return services;
        }

        /// <summary>
        /// Add and configure Swagger/OpenAPI documentation
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "Expense Tracker API",
                    Version = "1.0.0",
                    Description = "API Documentation for Expense Tracker",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Expense Tracker Team",
                        Email = "kgothalo.ramabele@lexisnexis.co.za"
                    }
                });

                // Enable XML comments for better API documentation
                var xmlFile = $"ExpenseTracker.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configure JWT authentication in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Configure enum handling to preserve names as strings
                c.SchemaFilter<EnumSchemaFilter>();
                c.UseAllOfToExtendReferenceSchemas();
            });

            return services;
        }

        /// <summary>
        /// Add and configure CORS policy
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("ExpenseTrackerCorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });

                // More restrictive policy for production
                options.AddPolicy("ExpenseTrackerCorsProduction", builder =>
                {
                    builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Add and configure JWT authentication
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        /// <summary>
        /// Initialize default data (categories, etc.)
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task InitializeExpenseTrackerDataAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            
            // Initialize default categories on startup
            await categoryService.InitializeDefaultCategoriesAsync();
        }
    }
}
