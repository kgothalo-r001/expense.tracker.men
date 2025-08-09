namespace Expense.Tracker.Services.Abstractions.Constants
{
    public static class ErrorMessages
    {
        public const string CategoryNotFound = "Category with ID '{0}' does not exist or does not belong to the current user.";
        public const string TransactionNotFound = "Transaction with ID '{0}' not found.";
        public const string UnauthorizedAccess = "Access denied for user '{0}'.";
        public const string InvalidRecurringSettings = "Invalid recurring transaction settings.";
        public const string TagDecrementFailed = "Failed to decrement tag usage for tag '{0}'.";
        public const string RecurringFrequencyRequired = "Recurring frequency is required for recurring transactions.";
        public const string RecurringEndDateInvalid = "Recurring end date must be after the transaction date.";
        public const string UnknownRecurringFrequency = "Unknown recurring frequency: {0}";
    }
}
