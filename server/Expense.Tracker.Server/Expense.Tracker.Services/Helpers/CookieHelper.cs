using Microsoft.AspNetCore.Http;

namespace Expense.Tracker.Services.Helpers
{
    public class CookieHelper
    {
        public static CookieOptions GetSecureCookieOptions(DateTime? expires = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires ?? DateTimeOffset.UtcNow.AddHours(24),
                Path = "/",
                Domain = null
            };
        }

        public static CookieOptions GetExpiredCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                Path = "/",
                Domain = null
            };
        }
    }
}
