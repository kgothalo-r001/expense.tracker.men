using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseTrackerController : ControllerBase
    {
        private readonly ILogger<ExpenseTrackerController> _logger;

        public ExpenseTrackerController(ILogger<ExpenseTrackerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult GetTest()
        {
            return Ok("Test endpoint is working");
        }

    }
}
