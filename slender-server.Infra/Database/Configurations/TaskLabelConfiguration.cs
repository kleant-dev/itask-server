using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskLabelConfiguration:IEntityTypeConfiguration<TaskLabel>
{
    public void Configure(EntityTypeBuilder<TaskLabel> builder)
    {
        builder.HasKey(tl => new { tl.TaskId, tl.LabelId });
        
        builder.Property(tl => tl.TaskId).HasMaxLength(50);
        builder.Property(tl => tl.LabelId).HasMaxLength(50);
        
        builder.HasOne(tl => tl.Task)
            .WithMany(t => t.Labels)
            .HasForeignKey(tl => tl.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(tl => tl.Label)
            .WithMany(l => l.TaskLabels)
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasIndex(tl => tl.LabelId);
        builder.HasIndex(tl => tl.TaskId);
        
        builder.HasQueryFilter(tl => tl.Task.DeletedAtUtc == null);
    }
}