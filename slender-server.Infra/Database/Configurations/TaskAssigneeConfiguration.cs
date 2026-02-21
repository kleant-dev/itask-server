using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskAssigneeConfiguration : IEntityTypeConfiguration<TaskAssignee>
{
    public void Configure(EntityTypeBuilder<TaskAssignee> builder)
    {
        builder.HasKey(ta => new { ta.TaskId, ta.UserId });
        
        builder.Property(ta => ta.TaskId).HasMaxLength(50);
        builder.Property(ta => ta.UserId).HasMaxLength(50);
        builder.Property(ta => ta.AssignedById).HasMaxLength(50).IsRequired(false);
        builder.Property(ta => ta.AssignedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(ta => ta.Task)
            .WithMany(t => t.Assignees)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(ta => ta.User)
            .WithMany()
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(ta => ta.AssignedBy)
            .WithMany()
            .HasForeignKey(ta => ta.AssignedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasIndex(ta => ta.UserId);
        builder.HasIndex(ta => ta.TaskId);
        
        builder.HasQueryFilter(ta => ta.Task.DeletedAtUtc == null);
    }
}