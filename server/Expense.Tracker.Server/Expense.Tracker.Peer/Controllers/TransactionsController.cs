using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Peer.Helpers;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Transactions}")]
    public class TransactionsController : ExpenseManagerBaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly ITelemetryHelper _telemetryHelper;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger, ITelemetryHelper telemetryHelper) 
            : base(logger)
        {
            _transactionService = transactionService;
            _telemetryHelper = telemetryHelper;
        }

        /// <summary>
        /// Get all transactions
        /// </summary>
        [HttpGet("GetTransactions")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(
            [FromQuery] string? categoryId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                IEnumerable<Transaction> transactions;

                if (!string.IsNullOrEmpty(categoryId))
                {
                    transactions = await _transactionService.GetTransactionsByCategoryAsync(categoryId, Requestor);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate.Value, endDate.Value, Requestor);
                }
                else
                {
                    transactions = await _transactionService.GetAllTransactionsAsync(Requestor);
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["CategoryId"] = categoryId ?? "null",
                    ["StartDate"] = startDate?.ToString("O") ?? "null",
                    ["EndDate"] = endDate?.ToString("O") ?? "null"
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetTransactions",
                    "TransactionsController.GetTransactions",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while retrieving transactions");
            }
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        [HttpGet("GetTransaction/{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(string id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id, Requestor);
                if (transaction == null)
                {
                    return NotFound($"Transaction with ID '{id}' not found");
                }
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction {TransactionId} for user {UserId}", id, Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving the transaction");
            }
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        [HttpPost("CreateTransaction")]
        public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transaction = await _transactionService.CreateTransactionAsync(request, Requestor);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "CreateTransaction",
                    "TransactionsController.CreateTransaction",
                    Requestor);

                return StatusCode(500, "An error occurred while creating the transaction");
            }
        }

        /// <summary>
        /// Update an existing transaction
        /// </summary>
        [HttpPut("UpdateTransaction/{id}")]
        public async Task<ActionResult<Transaction>> UpdateTransaction(string id, [FromBody] UpdateTransactionRequest request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest("Transaction ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTransaction = await _transactionService.UpdateTransactionAsync(request, Requestor);
                if (updatedTransaction == null)
                {
                    return NotFound($"Transaction with ID '{id}' not found");
                }

                return Ok(updatedTransaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", id, Requestor.UserId);
                return StatusCode(500, "An error occurred while updating the transaction");
            }
        }

        /// <summary>
        /// Delete a transaction
        /// </summary>
        [HttpDelete("DeleteTransaction/{id}")]
        public async Task<ActionResult> DeleteTransaction(string id)
        {
            try
            {
                var deleted = await _transactionService.DeleteTransactionAsync(id, Requestor);
                if (!deleted)
                {
                    return NotFound($"Transaction with ID '{id}' not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, Requestor.UserId);
                return StatusCode(500, "An error occurred while deleting the transaction");
            }
        }

        /// <summary>
        /// Get recurring transactions
        /// </summary>
        [HttpGet("GetRecurringTransactions")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetRecurringTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetRecurringTransactionsAsync(Requestor);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transactions for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving recurring transactions");
            }
        }

        /// <summary>
        /// Process recurring transactions (create new instances if due)
        /// </summary>
        [HttpPost("ProcessRecurringTransactions")]
        public async Task<ActionResult> ProcessRecurringTransactions()
        {
            try
            {
                await _transactionService.ProcessRecurringTransactionsAsync(Requestor);
                return Ok("Recurring transactions processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring transactions for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while processing recurring transactions");
            }
        }
    }
}
