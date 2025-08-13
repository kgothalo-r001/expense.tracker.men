using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring JWT authentication
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AuthenticationServiceExtensions
    {
        private const string JwtSecretConfigurationKey = "Jwt:Secret";

        /// <summary>
        /// Add and configure JWT authentication for the Expense Tracker application
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecret = configuration[JwtSecretConfigurationKey];
            if (string.IsNullOrWhiteSpace(jwtSecret))
                throw new InvalidOperationException($"JWT Secret not configured. Please set '{JwtSecretConfigurationKey}' in configuration.");
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = CreateTokenValidationParameters(key);
            });

            return services;
        }

        /// <summary>
        /// Create token validation parameters for JWT authentication
        /// </summary>
        /// <param name="key">The symmetric security key</param>
        /// <returns>Token validation parameters</returns>
        private static TokenValidationParameters CreateTokenValidationParameters(byte[] key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
