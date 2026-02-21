using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class WorkspaceInviteConfiguration : IEntityTypeConfiguration<WorkspaceInvite>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvite> builder)
    {
        builder.HasKey(wi => wi.Id);
        builder.Property(wi => wi.Id).HasMaxLength(50);
        
        builder.Property(wi => wi.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(wi => wi.InvitedByUserId).HasMaxLength(50).IsRequired();
        builder.Property(wi => wi.Email).HasMaxLength(255).IsRequired();
        
        builder.Property(wi => wi.Token).HasMaxLength(50).IsRequired();
        builder.HasIndex(wi => wi.Token).IsUnique();
        
        builder.Property(wi => wi.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(wi => wi.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.Property(wi => wi.ExpiresAtUtc).IsRequired();
        builder.Property(wi => wi.AcceptedAtUtc);
        
        builder.HasOne(wi => wi.Workspace)
            .WithMany(w => w.Invites)
            .HasForeignKey(wi => wi.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasOne(wi => wi.InvitedByUser)
            .WithMany()
            .HasForeignKey(wi => wi.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(wi => new { wi.WorkspaceId, wi.Email })
            .HasFilter("\"accepted_at_utc\" IS NULL") 
            .IsUnique();
        
        builder.HasQueryFilter(wi => wi.InvitedByUser.DeletedAtUtc == null);
    }
}