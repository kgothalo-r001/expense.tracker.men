using System;

namespace Expense.Tracker.Services.Exceptions
{
    public class CategoryNotFoundException : Exception
    {
        public CategoryNotFoundException(string categoryId) 
            : base($"Category with ID '{categoryId}' does not exist or does not belong to the current user.")
        {
        }

        public CategoryNotFoundException(string categoryId, string message) 
            : base(message)
        {
        }
    }

    public class InvalidRecurringTransactionException : Exception
    {
        public InvalidRecurringTransactionException(string message) 
            : base(message)
        {
        }
    }
}
