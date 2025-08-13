using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Expense.Tracker.Services.Data;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Repositories;
using Expense.Tracker.Services.Helpers;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Moq;

namespace Expense.Tracker.Tests.Helpers;

public abstract class BaseTestHelper
{
    protected Guid TestUserId { get; } = Guid.NewGuid();
    /// <summary>
    /// Provides a default HttpContext for controller tests to avoid null reference errors.
    /// </summary>
    protected DefaultHttpContext DefaultHttpContext { get; } = new DefaultHttpContext();
}
