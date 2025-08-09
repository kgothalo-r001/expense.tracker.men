using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync(Requestor requestor)
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(string id, Requestor requestor)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag> CreateTagAsync(CreateTagRequest request, Requestor requestor)
        {
            var existingTag = await _tagRepository.GetByNameAsync(request.Name);
            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag with name '{request.Name}' already exists.");
            }

            var tag = new Tag
            {
                Name = request.Name,
                Color = request.Color,
                UsageCount = 0
            };

            return await _tagRepository.CreateAsync(tag);
        }

        public async Task<bool> DeleteTagAsync(string id, Requestor requestor)
        {
            return await _tagRepository.DeleteAsync(id);
        }

        public async Task UpdateTagUsageAsync(string tagName, Requestor requestor)
        {
            await _tagRepository.IncrementUsageAsync(tagName);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(Requestor requestor, int limit = 10)
        {
            return await _tagRepository.GetPopularAsync(limit);
        }
    }
}
