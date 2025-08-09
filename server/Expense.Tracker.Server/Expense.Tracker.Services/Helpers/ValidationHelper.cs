namespace Expense.Tracker.Services.Helpers
{
    public static class ValidationHelper
    {
        public static class ErrorMessages
        {
            public const string CategoryNameRequired = "Category name is required.";
            public const string CategoryAlreadyExists = "Category with name '{0}' already exists.";
            public const string CannotDeleteDefaultCategory = "Cannot delete default categories.";
        }

        public static void ValidateString(string? value, string paramName, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(errorMessage, paramName);
            }
        }

        public static void ValidateNotNull<T>(T? value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
