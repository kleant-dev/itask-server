using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasMaxLength(50);
        
        builder.Property(l => l.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Color).HasMaxLength(7).IsRequired();
        builder.Property(l => l.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(l => l.Workspace)
            .WithMany()
            .HasForeignKey(l => l.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(l => l.WorkspaceId);
        builder.HasIndex(l => new { l.WorkspaceId, l.Name }).IsUnique();
        
        builder.HasQueryFilter(l => l.Workspace.Owner.DeletedAtUtc == null);
    }
}