using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IAuthenticatedUserHelper _userHelper;

        public TagService(ITagRepository tagRepository, IAuthenticatedUserHelper userHelper)
        {
            _tagRepository = tagRepository;
            _userHelper = userHelper;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);

            return await _tagRepository.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(string id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag?> GetUserTagByIdAsync(string id, Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);

            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag> CreateTagAsync(CreateTagRequest request)
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

        public async Task<bool> DeleteTagAsync(string id)
        {
            return await _tagRepository.DeleteAsync(id);
        }

        public async Task UpdateTagUsageAsync(string tagName)
        {
            await _tagRepository.IncrementUsageAsync(tagName);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10)
        {
            return await _tagRepository.GetPopularAsync(limit);
        }

        public async Task<IEnumerable<Tag>> GetUserPopularTagsAsync(Guid userId, int limit = 10)
        {
            _userHelper.ValidateUserAccess(userId);
            return await _tagRepository.GetPopularAsync(limit);
        }
    }
}
