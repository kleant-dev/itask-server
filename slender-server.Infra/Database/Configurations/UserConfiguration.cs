using slender_server.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasMaxLength(50).ValueGeneratedNever();

        builder.Property(u => u.IdentityId).HasMaxLength(500).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarColor).HasMaxLength(7);
        builder.Property(u=>u.AvatarUrl).HasMaxLength(500);
        builder.Property(u=>u.DisplayName).HasMaxLength(100);
        
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IdentityId).IsUnique();

        builder.Property(u => u.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasQueryFilter(u => u.DeletedAtUtc == null);
    }
}