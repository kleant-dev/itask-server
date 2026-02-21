using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskAttachmentConfiguration : IEntityTypeConfiguration<TaskAttachment>
{
    public void Configure(EntityTypeBuilder<TaskAttachment> builder)
    {
        builder.HasKey(ta => ta.Id);
        builder.Property(ta => ta.Id).HasMaxLength(50);
        
        builder.Property(ta => ta.TaskId).HasMaxLength(50).IsRequired();
        builder.Property(ta => ta.UploadedById).HasMaxLength(50).IsRequired();
        builder.Property(ta => ta.FileName).HasMaxLength(255).IsRequired();
        builder.Property(ta => ta.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(ta => ta.FileSizeBytes).IsRequired();
        builder.Property(ta => ta.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(ta => ta.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(ta => ta.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(ta=>ta.UploadedBy)
            .WithMany()
            .HasForeignKey(ta => ta.UploadedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(ta => ta.TaskId);
        
        builder.HasQueryFilter(ta => ta.Task.DeletedAtUtc == null);
    }
}