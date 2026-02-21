using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskCommentAttachmentConfiguration : IEntityTypeConfiguration<TaskCommentAttachment>
{
    public void Configure(EntityTypeBuilder<TaskCommentAttachment> builder)
    {
        builder.HasKey(tca => tca.Id);
        builder.Property(tca => tca.Id).HasMaxLength(50);
        
        builder.Property(tca => tca.CommentId).HasMaxLength(50).IsRequired();
        builder.Property(tca => tca.UploadedById).HasMaxLength(50).IsRequired();
        builder.Property(tca => tca.FileName).HasMaxLength(255).IsRequired();
        builder.Property(tca => tca.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(tca => tca.FileSizeBytes).IsRequired();
        builder.Property(tca => tca.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(tca => tca.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(tca => tca.TaskComment)
            .WithMany(tc => tc.Attachments)
            .HasForeignKey(tca => tca.CommentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(tca => tca.UploadedBy)
            .WithMany()
            .HasForeignKey(tca => tca.UploadedById)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasIndex(tca => tca.CommentId);
        
        builder.HasQueryFilter(tca => tca.TaskComment.DeletedAtUtc == null);
    }
}