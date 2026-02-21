using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasMaxLength(50);
        
        builder.Property(n => n.RecipientId).HasMaxLength(50).IsRequired();
        builder.Property(n => n.ActorId).HasMaxLength(50);
        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(n => n.WorkspaceId).HasMaxLength(50);
        builder.Property(n => n.ProjectId).HasMaxLength(50);
        builder.Property(n => n.TaskId).HasMaxLength(50);
        
        builder.Property(n => n.Title).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasColumnType("text");
        builder.Property(n => n.EntityName).HasMaxLength(500);
        
        builder.Property(n => n.ReadAtUtc);
        builder.Property(n => n.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.ActorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(n => new { n.RecipientId, n.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_Recipient_Created")
            .IsDescending(false, true); 
        
        builder.HasIndex(n => n.RecipientId)
            .HasFilter("\"read_at_utc\" IS NULL")
            .HasDatabaseName("IX_Notifications_Unread");
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Notification_SingleTarget",
                """
                (CASE WHEN "workspace_id" IS NOT NULL THEN 1 ELSE 0 END +
                                   CASE WHEN "project_id" IS NOT NULL THEN 1 ELSE 0 END +
                                   CASE WHEN "task_id" IS NOT NULL THEN 1 ELSE 0 END) <= 1
                """
            );
        });
    }
}