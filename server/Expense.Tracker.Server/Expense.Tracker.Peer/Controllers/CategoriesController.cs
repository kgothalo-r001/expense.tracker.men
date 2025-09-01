using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Peer.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Categories}")]
    public class CategoriesController : ExpenseManagerBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ITelemetryHelper _telemetryHelper;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger, ITelemetryHelper telemetryHelper) 
            : base(logger)
        {
            _categoryService = categoryService;
            _telemetryHelper = telemetryHelper;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("GetCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(Requestor);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetCategories",
                    "CategoriesController.GetCategories",
                    Requestor);

                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("GetCategory/{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id, Requestor);
                if (category == null)
                {
                    return NotFound($"Category with ID '{id}' not found");
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["CategoryId"] = id
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetCategory",
                    "CategoriesController.GetCategory",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while retrieving the category");
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost("CreateCategory")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = await _categoryService.CreateCategoryAsync(request, Requestor);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "CreateCategory",
                    "CategoriesController.CreateCategory",
                    Requestor);

                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("UpdateCategory/{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest("Category ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCategory = await _categoryService.UpdateCategoryAsync(request, Requestor);
                if (updatedCategory == null)
                {
                    return NotFound($"Category with ID '{id}' not found");
                }

                return Ok(updatedCategory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["CategoryId"] = id
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "UpdateCategory",
                    "CategoriesController.UpdateCategory",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while updating the category");
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("DeleteCategory/{id}")]
        public async Task<ActionResult> DeleteCategory(string id)
        {
            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id, Requestor);
                if (!deleted)
                {
                    return NotFound($"Category with ID '{id}' not found");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["CategoryId"] = id
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "DeleteCategory",
                    "CategoriesController.DeleteCategory",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while deleting the category");
            }
        }

        /// <summary>
        /// Initialize default categories
        /// </summary>
        [HttpPost("InitializeDefaultCategories")]
        public async Task<ActionResult> InitializeDefaultCategories()
        {
            try
            {
                await _categoryService.InitializeDefaultCategoriesAsync(Requestor);
                return Ok("Default categories initialized successfully");
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "InitializeDefaultCategories",
                    "CategoriesController.InitializeDefaultCategories",
                    Requestor);

                return StatusCode(500, "An error occurred while initializing default categories");
            }
        }
    }
}
