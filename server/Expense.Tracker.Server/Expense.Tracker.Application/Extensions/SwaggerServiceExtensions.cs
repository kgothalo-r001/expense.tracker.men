using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring Swagger/OpenAPI documentation
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SwaggerServiceExtensions
    {
        private const string ExpenseTrackerXmlFileName = "ExpenseTracker.xml";

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
                c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Expense Tracker API",
                    Version = "1.0.0",
                    Description = "API Documentation for Expense Tracker",
                    Contact = new OpenApiContact
                    {
                        Name = "Expense Tracker Team",
                        Email = "kgothalo.ramabele@lexisnexis.co.za"
                    }
                });

                // Enable XML comments for better API documentation
                var xmlPath = Path.Combine(AppContext.BaseDirectory, ExpenseTrackerXmlFileName);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configure JWT authentication in Swagger
                ConfigureSwaggerSecurity(c);

                // Configure enum handling to preserve names as strings
                c.SchemaFilter<EnumSchemaFilter>();
                c.UseAllOfToExtendReferenceSchemas();
            });

            return services;
        }

        /// <summary>
        /// Configure JWT authentication security scheme in Swagger
        /// </summary>
        /// <param name="swaggerGenOptions">The Swagger generation options</param>
        private static void ConfigureSwaggerSecurity(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        }
    }
}
