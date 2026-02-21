using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(50);
        
        builder.Property(t => t.ProjectId).HasMaxLength(50).IsRequired();
        builder.Property(t => t.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(t => t.CreatedById).HasMaxLength(50).IsRequired(false);
        builder.Property(t => t.ParentTaskId).HasMaxLength(50);
        builder.Property(t => t.Title).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Description).HasColumnType("text");
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(t => t.DueDate).HasColumnType("date");
        builder.Property(t => t.SortOrder)
            .HasColumnType("double precision")
            .IsRequired();
        builder.Property(t => t.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Workspace)
            .WithMany()
            .HasForeignKey(t => t.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => new { t.WorkspaceId, t.Status });
        builder.HasIndex(t => new { t.ProjectId, t.Status, t.SortOrder });
        builder.HasIndex(t => t.ScheduledAt)
            .HasFilter("\"scheduled_at\" IS NOT NULL");
        builder.HasIndex(t => t.ParentTaskId)
            .HasFilter("\"parent_task_id\" IS NOT NULL");
        builder.HasIndex(t => t.CreatedById);
        
        builder.HasQueryFilter(t => t.DeletedAtUtc == null);
    }
}