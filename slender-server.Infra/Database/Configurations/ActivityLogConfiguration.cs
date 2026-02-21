using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id).HasMaxLength(50).ValueGeneratedNever();
        
        builder.Property(al => al.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(al => al.ActorId).HasMaxLength(50).IsRequired();
        builder.Property(al => al.Action).HasMaxLength(100).IsRequired();
        builder.Property(al => al.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(al => al.EntityId).HasMaxLength(50).IsRequired();
        builder.Property(al => al.OldValue).HasColumnType("text");
        builder.Property(al => al.NewValue).HasColumnType("text");
        
        builder.Property(al => al.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(al => al.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(al => al.ActorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(al => al.WorkspaceId);
        builder.HasIndex(al => al.ActorId);
        builder.HasIndex(al => new { al.WorkspaceId, al.CreatedAtUtc })
            .HasDatabaseName("IX_ActivityLogs_Workspace_CreatedAt")
            .IsDescending(false, true);
        builder.HasIndex(al => new { al.EntityType, al.EntityId })
            .HasDatabaseName("IX_ActivityLogs_Entity");
        builder.HasIndex(al => al.CreatedAtUtc)
            .HasDatabaseName("IX_ActivityLogs_CreatedAt")
            .IsDescending(true);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ActivityLog_ActionNotEmpty",
                """LENGTH(TRIM("action")) > 0"""
            );
            t.HasCheckConstraint(
                "CK_ActivityLog_EntityTypeNotEmpty",
                """LENGTH(TRIM("entity_type")) > 0"""
            );
            t.HasCheckConstraint(
                "CK_ActivityLog_EntityIdNotEmpty",
                """LENGTH(TRIM("entity_id")) > 0"""
            );
        });
    }
}
