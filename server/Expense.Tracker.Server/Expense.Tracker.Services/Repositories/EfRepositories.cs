using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Data;

namespace Expense.Tracker.Services.Repositories
{
    public class EfCategoryRepository : ICategoryRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public EfCategoryRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(string id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> CreateAsync(Category entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Set to existing sample user ID if not provided (must reference existing user)
            if (string.IsNullOrEmpty(entity.UserId))
            {
                entity.UserId = "550e8400-e29b-41d4-a716-446655440000"; // Sample user ID from database
            }
            
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Category?> UpdateAsync(Category entity)
        {
            var existing = await _context.Categories.FindAsync(entity.Id);
            if (existing == null)
                return null;

            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.Color = entity.Color;
            existing.Icon = entity.Icon;
            existing.Type = entity.Type;
            existing.IsDefault = entity.IsDefault;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null)
                return false;

            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetByTypeAsync(CategoryType type)
        {
            return await _context.Categories
                .Where(c => c.Type == type)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId.ToString())
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetByUserIdAndTypeAsync(Guid userId, CategoryType type)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId.ToString() && c.Type == type)
                .ToListAsync();
        }

        public async Task<Category?> GetByUserIdAndNameAsync(Guid userId, string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId.ToString() && c.Name == name);
        }

        public async Task<Category?> GetByUserIdAndIdAsync(Guid userId, string id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId.ToString() && c.Id == id);
        }
    }

    public class EfTransactionRepository : ITransactionRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public EfTransactionRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            // Note: This method should not be used directly. Use user-specific methods instead.
            // This is kept for backward compatibility but should be avoided.
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .ToListAsync();
                
            // For now, set empty tags list since we're ignoring the Tags property
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId.ToString())
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<Transaction?> GetByIdAsync(string id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (transaction != null)
            {
                transaction.Tags = new List<string>();
            }
            
            return transaction;
        }

        public async Task<Transaction?> GetByUserIdAndIdAsync(Guid userId, string id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId.ToString());
                
            if (transaction != null)
            {
                transaction.Tags = new List<string>();
            }
            
            return transaction;
        }

        public async Task<Transaction> CreateAsync(Transaction entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            if (string.IsNullOrEmpty(entity.UserId))
            {
                entity.UserId = "550e8400-e29b-41d4-a716-446655440000";
            }
            
            _context.Transactions.Add(entity);
            await _context.SaveChangesAsync();
            
            // Reload with category
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task<Transaction?> UpdateAsync(Transaction entity)
        {
            var existing = await _context.Transactions.FindAsync(entity.Id);
            if (existing == null)
                return null;

            existing.Amount = entity.Amount;
            existing.Description = entity.Description;
            existing.Date = entity.Date;
            existing.Type = entity.Type;
            existing.CategoryId = entity.CategoryId;
            existing.Notes = entity.Notes;
            existing.IsRecurring = entity.IsRecurring;
            existing.RecurringFrequency = entity.RecurringFrequency;
            existing.RecurringEndDate = entity.RecurringEndDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(existing.Id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.Transactions.FindAsync(id);
            if (entity == null)
                return false;

            _context.Transactions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Transactions.AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(string categoryId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAndCategoryIdAsync(Guid userId, string categoryId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId.ToString() && t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId.ToString() && t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetRecurringAsync()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.IsRecurring)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetRecurringByUserIdAsync(Guid userId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.IsRecurring && t.UserId == userId.ToString())
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<decimal> GetTotalAmountByTypeAsync(TransactionType type, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Transactions.Where(t => t.Type == type);
            
            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return await query.SumAsync(t => t.Amount);
        }

        public async Task<IEnumerable<Transaction>> GetRecentAsync(int limit)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .Take(limit)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int limit)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId.ToString())
                .OrderByDescending(t => t.Date)
                .Take(limit)
                .ToListAsync();
                
            foreach (var transaction in transactions)
            {
                transaction.Tags = new List<string>();
            }
            
            return transactions;
        }
    }

    public class EfTagRepository : ITagRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public EfTagRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _context.Tags.ToListAsync();
        }

        public async Task<Tag?> GetByIdAsync(string id)
        {
            return await _context.Tags.FindAsync(id);
        }

        public async Task<Tag> CreateAsync(Tag entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            
            _context.Tags.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Tag?> UpdateAsync(Tag entity)
        {
            var existing = await _context.Tags.FindAsync(entity.Id);
            if (existing == null)
                return null;

            existing.Name = entity.Name;
            existing.Color = entity.Color;
            existing.UsageCount = entity.UsageCount;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.Tags.FindAsync(id);
            if (entity == null)
                return false;

            _context.Tags.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Tags.AnyAsync(t => t.Id == id);
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<IEnumerable<Tag>> GetPopularAsync(int limit)
        {
            return await _context.Tags
                .OrderByDescending(t => t.UsageCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task IncrementUsageAsync(string tagName)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName);
                
            if (tag != null)
            {
                tag.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}
