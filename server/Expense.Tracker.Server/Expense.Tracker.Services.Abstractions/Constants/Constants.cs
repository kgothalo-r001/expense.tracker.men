using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Constants
{
    public static class ApiConstants
    {
        public const string BaseApiRoute = "/f";
        
        public static class Routes
        {
            public const string Auth = "auth";
            public const string Categories = "categories";
            public const string Transactions = "transactions";
            public const string Dashboard = "dashboard";
            public const string Tags = "tags";
            public const string Analytics = "analytics";
        }
        
        public static class DefaultCategories
        {
            public static readonly (string Name, string Color, string Icon, CategoryType Type)[] Categories = 
            {
                ("Food & Dining", "#FF6B6B", "restaurant", CategoryType.EXPENSE),
                ("Transportation", "#4ECDC4", "directions_car", CategoryType.EXPENSE),
                ("Shopping", "#45B7D1", "shopping_cart", CategoryType.EXPENSE),
                ("Entertainment", "#96CEB4", "movie", CategoryType.EXPENSE),
                ("Bills & Utilities", "#FFEAA7", "receipt", CategoryType.EXPENSE),
                ("Healthcare", "#DDA0DD", "local_hospital", CategoryType.EXPENSE),
                ("Salary", "#98D8C8", "work", CategoryType.INCOME),
                ("Freelance", "#F7DC6F", "laptop", CategoryType.INCOME),
                ("Investment", "#BB8FCE", "trending_up", CategoryType.INCOME)
            };
        }
    }

    public static class BusinessConstants
    {
        public const int RecentTransactionsLimit = 10;
        public const int TopCategoriesLimit = 5;
        public const int AnalyticsPeriodMonths = 12;
        public const decimal DefaultBudgetBuffer = 0.1m; // 10% buffer for projections
    }
}
