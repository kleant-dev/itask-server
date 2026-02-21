using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasMaxLength(50);
        
        builder.Property(p => p.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(p => p.OwnerId).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(p => p.Description).HasColumnType("text");
        builder.Property(p => p.Color).HasMaxLength(7);
        builder.Property(p => p.Icon).HasMaxLength(500);
        builder.Property(p => p.StartDate).HasColumnType("date");
        builder.Property(p => p.TargetDate).HasColumnType("date");
        builder.Property(p => p.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(p=>p.Workspace)
            .WithMany(w => w.Projects)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(p=>p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(p => p.WorkspaceId);
        builder.HasIndex(p => new { p.WorkspaceId, p.Status });
        
        builder.HasQueryFilter(p => p.Owner.DeletedAtUtc == null);
    }
}