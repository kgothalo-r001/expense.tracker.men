using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Peer.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class ExpenseManagerBaseController : ControllerBase, IActionFilter
    {
        protected readonly ILogger _logger;
        protected Requestor Requestor { get; private set; }

        protected ExpenseManagerBaseController(ILogger logger)
        {
            _logger = logger;
        }

        [NonAction]
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            InitializeRequestor();
            
            if (!IsAccessValid())
            {
                context.Result = BadRequest("Invalid session or user context");
                return;
            }
        }

        [NonAction]
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private void InitializeRequestor()
        {
            try
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionIdClaim = User?.FindFirst("SessionId")?.Value;
                
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    Requestor = new Requestor
                    {
                        UserId = userIdClaim,
                        SessionId = sessionIdClaim ?? string.Empty
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to extract user information from claims");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing requestor context");
            }
        }

        private bool IsAccessValid()
        {
            return Requestor != null && !string.IsNullOrEmpty(Requestor.UserId);
        }
    }
}
