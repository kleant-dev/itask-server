using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class WorkspaceConfiguration:IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasMaxLength(50).ValueGeneratedNever();
        
        builder.Property(w => w.OwnerId).HasMaxLength(50).IsRequired();
        builder.Property(w=>w.Name).HasMaxLength(100).IsRequired();
        builder.Property(w => w.Slug).HasMaxLength(60).IsRequired();
        builder.Property(w => w.LogoUrl).HasMaxLength(500);
        builder.Property(w => w.Description).HasColumnType("text");
        builder.Property(w => w.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.HasOne(w=>w.Owner)
            .WithMany()
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(w => w.Slug).IsUnique();
        builder.HasIndex(w => w.OwnerId);
        
        builder.HasQueryFilter(w => w.Owner.DeletedAtUtc == null);
    }
}