using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database.Configurations;

public sealed class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.HasKey(tc => tc.Id);
        builder.Property(tc => tc.Id).HasMaxLength(50);
        
        builder.Property(tc => tc.TaskId).HasMaxLength(50).IsRequired();
        builder.Property(tc => tc.AuthorId).HasMaxLength(50).IsRequired(false);
        builder.Property(tc => tc.ParentCommentId).HasMaxLength(50);
        builder.Property(tc => tc.Body).HasColumnType("text").IsRequired();
        
        builder.Property(tc => tc.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        builder.Property(tc => tc.UpdatedAtUtc).IsRequired();
        builder.Property(tc => tc.DeletedAtUtc);
        
        builder.HasOne(tc => tc.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(tc => tc.Author)
            .WithMany()
            .HasForeignKey(tc => tc.AuthorId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        builder.HasOne(tc => tc.ParentComment)
            .WithMany(tc => tc.Replies)
            .HasForeignKey(tc => tc.ParentCommentId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(tc => tc.TaskId);
        builder.HasIndex(tc => tc.AuthorId);
        builder.HasIndex(tc => tc.ParentCommentId)
            .HasFilter("\"parent_comment_id\" IS NOT NULL");
        
        builder.HasQueryFilter(tc => tc.DeletedAtUtc == null);
    }
}