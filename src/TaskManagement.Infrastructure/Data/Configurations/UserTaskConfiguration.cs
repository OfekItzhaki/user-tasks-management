using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core fluent configuration for UserTask junction entity
/// </summary>
public class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
{
    public void Configure(EntityTypeBuilder<UserTask> entity)
    {
        entity.HasKey(e => new { e.TaskId, e.UserId });
        entity.Property(e => e.Role).IsRequired().HasConversion<int>();
        entity.Property(e => e.AssignedAt).IsRequired();

        entity.HasOne(ut => ut.Task)
            .WithMany(t => t.UserTasks)
            .HasForeignKey(ut => ut.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ut => ut.User)
            .WithMany(u => u.UserTasks)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
