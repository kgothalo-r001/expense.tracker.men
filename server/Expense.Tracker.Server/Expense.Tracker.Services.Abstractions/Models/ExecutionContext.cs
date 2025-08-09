namespace Expense.Tracker.Services.Abstractions.Models
{
    public interface IExecutionContext
    {
        bool AccessAllowed { get; }
        Requestor Requestor { get; }
    }

    public class ExecutionContext : IExecutionContext
    {
        public bool AccessAllowed { get; set; }
        public Requestor Requestor { get; set; } = new();
    }

    public class Requestor
    {
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}
