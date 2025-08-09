using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Categories}")]
    public class CategoriesController : ExpenseManagerBaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger) 
            : base(logger)
        {
            _categoryService = categoryService;
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
                _logger.LogError(ex, "Error retrieving categories for user {UserId}", Requestor.UserId);
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
                _logger.LogError(ex, "Error retrieving category {CategoryId} for user {UserId}", id, Requestor.UserId);
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
                _logger.LogError(ex, "Error creating category for user {UserId}", Requestor.UserId);
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
                _logger.LogError(ex, "Error updating category {CategoryId} for user {UserId}", id, Requestor.UserId);
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
                _logger.LogError(ex, "Error deleting category {CategoryId} for user {UserId}", id, Requestor.UserId);
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
                _logger.LogError(ex, "Error initializing default categories for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while initializing default categories");
            }
        }
    }
}
