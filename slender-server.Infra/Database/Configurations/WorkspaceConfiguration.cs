using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasMaxLength(50).ValueGeneratedNever();
        builder.Property(w => w.OwnerId).HasMaxLength(50).IsRequired();
        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();
        builder.Property(w => w.Slug).HasMaxLength(60).IsRequired();
        builder.Property(w => w.LogoUrl).HasMaxLength(500);
        builder.Property(w => w.Description).HasColumnType("text");
        builder.Property(w => w.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.HasOne(w => w.Owner)
            .WithMany()
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Unique slug enforced at DB level — the real protection against slug race conditions.
        // The unique index here means that even if two concurrent requests both pass the
        // application-level SlugExistsAsync check, only one INSERT will succeed.
        // Catch DbUpdateException in the service layer and map it to a domain error.
        builder.HasIndex(w => w.Slug)
            .IsUnique()
            .HasFilter("\"deleted_at_utc\" IS NULL");

        builder.HasIndex(w => w.OwnerId);

        // Soft-delete global query filter: excludes workspaces that have been deleted
        // OR whose owner has been deleted. Applied automatically to all EF queries.
        builder.HasQueryFilter(w => w.DeletedAtUtc == null && w.Owner.DeletedAtUtc == null);
    }
}