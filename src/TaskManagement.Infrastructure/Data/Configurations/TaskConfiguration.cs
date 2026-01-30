using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskEntity = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core fluent configuration for Task entity
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        entity.Property(e => e.DueDate).IsRequired();
        entity.Property(e => e.Priority).IsRequired().HasConversion<int>();
        entity.Property(e => e.CreatedByUserId).IsRequired();
        entity.Property(e => e.CreatedAt).IsRequired();
        entity.Property(e => e.UpdatedAt).IsRequired();
        entity.Property(e => e.RowVersion).IsRowVersion().IsConcurrencyToken();

        entity.HasIndex(e => e.DueDate).HasDatabaseName("IX_Tasks_DueDate");
        entity.HasIndex(e => e.Priority).HasDatabaseName("IX_Tasks_Priority");
        entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Tasks_CreatedAt");
        entity.HasIndex(e => e.Title).HasDatabaseName("IX_Tasks_Title");
        entity.HasIndex(e => e.Description).HasDatabaseName("IX_Tasks_Description");

        entity.HasMany(e => e.UserTasks)
            .WithOne(ut => ut.Task)
            .HasForeignKey(ut => ut.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.TaskTags)
            .WithOne(tt => tt.Task)
            .HasForeignKey(tt => tt.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
