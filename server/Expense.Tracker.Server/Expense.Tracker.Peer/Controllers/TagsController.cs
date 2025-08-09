using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Tags}")]
    public class TagsController : ExpenseManagerBaseController
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService, ILogger<TagsController> logger)
            : base(logger)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        [HttpGet("GetTags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync(Requestor);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving tags");
            }
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        [HttpGet("GetTag/{id}")]
        public async Task<ActionResult<Tag>> GetTag(string id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id, Requestor);
                if (tag == null)
                {
                    return NotFound($"Tag with ID '{id}' not found");
                }
                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag {TagId} for user {UserId}", id, Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving the tag");
            }
        }

        /// <summary>
        /// Create a new tag
        /// </summary>
        [HttpPost("CreateTag")]
        public async Task<ActionResult<Tag>> CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tag = await _tagService.CreateTagAsync(request, Requestor);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while creating the tag");
            }
        }

        /// <summary>
        /// Delete a tag
        /// </summary>
        [HttpDelete("DeleteTag/{id}")]
        public async Task<ActionResult> DeleteTag(string id)
        {
            try
            {
                var deleted = await _tagService.DeleteTagAsync(id, Requestor);
                if (!deleted)
                {
                    return NotFound($"Tag with ID '{id}' not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId} for user {UserId}", id, Requestor.UserId);
                return StatusCode(500, "An error occurred while deleting the tag");
            }
        }

        /// <summary>
        /// Get popular tags
        /// </summary>
        [HttpGet("GetPopularTags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetPopularTags([FromQuery] int limit = 10)
        {
            try
            {
                var tags = await _tagService.GetPopularTagsAsync(Requestor, limit);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular tags for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving popular tags");
            }
        }
    }
}
