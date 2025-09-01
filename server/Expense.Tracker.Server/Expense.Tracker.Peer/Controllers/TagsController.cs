using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Peer.Helpers;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Tags}")]
    public class TagsController : ExpenseManagerBaseController
    {
        private readonly ITagService _tagService;
        private readonly ITelemetryHelper _telemetryHelper;

        public TagsController(ITagService tagService, ILogger<TagsController> logger, ITelemetryHelper telemetryHelper)
            : base(logger)
        {
            _tagService = tagService;
            _telemetryHelper = telemetryHelper;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        [HttpGet("GetTags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetTags",
                    "TagsController.GetTags",
                    Requestor);

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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["TagId"] = id
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetTag",
                    "TagsController.GetTag",
                    Requestor,
                    additionalProperties);

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
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "CreateTag",
                    "TagsController.CreateTag",
                    Requestor);

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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["TagId"] = id
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "DeleteTag",
                    "TagsController.DeleteTag",
                    Requestor,
                    additionalProperties);

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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["Limit"] = limit.ToString()
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetPopularTags",
                    "TagsController.GetPopularTags",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while retrieving popular tags");
            }
        }
    }
}
