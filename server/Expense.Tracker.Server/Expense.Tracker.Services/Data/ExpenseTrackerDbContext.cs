using Microsoft.EntityFrameworkCore;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Data
{
    public class ExpenseTrackerDbContext : DbContext
    {
        public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.Color).HasColumnName("color").IsRequired().HasMaxLength(7);
                entity.Property(e => e.Icon).HasColumnName("icon").HasMaxLength(50);
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

                // Map UserId property to user_id column
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired().HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(12,2)");
                entity.Property(e => e.Description).HasColumnName("description").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(1000);
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(e => e.Date).HasColumnName("transaction_date").IsRequired().HasColumnType("date");
                entity.Property(e => e.IsRecurring).HasColumnName("is_recurring").HasDefaultValue(false);
                entity.Property(e => e.RecurringFrequency)
                    .HasColumnName("recurring_frequency")
                    .HasConversion<string>();
                entity.Property(e => e.RecurringEndDate).HasColumnName("recurring_end_date").HasColumnType("date");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
                
                // Map UserId property to user_id column
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());

                // Ignore the Tags property since it's handled by the many-to-many relationship
                entity.Ignore(e => e.Tags);

                // Foreign key relationship - explicitly specify the foreign key property
                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Amount);
                entity.HasIndex(e => e.IsRecurring);
            });

            // Configure Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(7);
                entity.Property(e => e.UsageCount).HasColumnName("usage_count").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                // Add shadow property for user_id since the C# model doesn't have it
                entity.Property<string>("user_id").HasColumnName("user_id").HasMaxLength(36)
                    .HasConversion(
                        v => Guid.Parse(v),
                        v => v.ToString());

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.UsageCount);
            });

            // Remove the duplicate ToTable calls since they're now included above
        }
    }
}
