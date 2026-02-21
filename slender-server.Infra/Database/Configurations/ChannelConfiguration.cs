using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasMaxLength(50);
        
        builder.Property(c => c.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(c => c.CreatedById).HasMaxLength(50);
        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(c => c.Name).HasMaxLength(80);
        builder.Property(c => c.ProjectId).HasMaxLength(50);
        builder.Property(c => c.ParticipantHash)
            .HasMaxLength(64)
            .IsFixedLength();
        
        builder.Property(c => c.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne(c => c.Workspace)
            .WithMany()
            .HasForeignKey(c => c.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasOne(c=>c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(c => new { c.WorkspaceId, c.CreatedAtUtc })
            .IsDescending(false, true);
        builder.HasIndex(c => new { c.WorkspaceId, c.ProjectId })
            .HasFilter("\"project_id\" IS NOT NULL");
        builder.HasIndex(c => c.WorkspaceId)
            .HasFilter("\"type\" = 'DirectMessage'");
        builder.HasIndex(c => c.ParticipantHash)
            .HasFilter("\"type\" = 'DirectMessage'");
        builder.HasIndex(c => new { c.WorkspaceId, c.ParticipantHash })
            .IsUnique()
            .HasFilter("\"participant_hash\" IS NOT NULL");
        builder.HasIndex(c => new { c.WorkspaceId, c.Name })
            .IsUnique()
            .HasFilter("\"name\" IS NOT NULL");
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Channel_NameRules",
                """
                ("type" = 'DirectMessage' AND "name" IS NULL) OR 
                ("type" IN ('Public', 'Private') AND "name" IS NOT NULL)
                """
            );
            t.HasCheckConstraint(
                "CK_Channel_ProjectScope",
                """
                ("type" = 'DirectMessage' AND "project_id" IS NULL) OR 
                ("type" IN ('Public', 'Private'))
                """
            );
            t.HasCheckConstraint(
                "CK_Channel_ParticipantHashOnlyForDMs",
                """
                ("type" = 'DirectMessage' AND "participant_hash" IS NOT NULL) OR 
                ("type" IN ('Public', 'Private') AND "participant_hash" IS NULL)
                """
            );
        });
    }
}