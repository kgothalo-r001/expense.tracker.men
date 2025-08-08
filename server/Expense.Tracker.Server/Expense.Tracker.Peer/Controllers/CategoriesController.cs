using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [ApiController]
    [Authorize]
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Categories}")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("GetCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
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
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound($"Category with ID '{id}' not found");
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
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

                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
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

                var updatedCategory = await _categoryService.UpdateCategoryAsync(request);
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
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
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
                var deleted = await _categoryService.DeleteCategoryAsync(id);
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
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
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
                await _categoryService.InitializeDefaultCategoriesAsync();
                return Ok("Default categories initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default categories");
                return StatusCode(500, "An error occurred while initializing default categories");
            }
        }
    }
}
