using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.HasKey(wm => new { wm.WorkspaceId, wm.UserId });
        builder.Property(wm => wm.WorkspaceId).HasMaxLength(50);
        builder.Property(wm => wm.UserId).HasMaxLength(50);
        builder.Property(wm => wm.InvitedByUserId).HasMaxLength(50).IsRequired(false);
        builder.Property(wm => wm.Role)
            .HasConversion<string>() 
            .HasMaxLength(20);
        builder.Property(wm => wm.JoinedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(wm => wm.Workspace)
            .WithMany(w => w.Members)
            .HasForeignKey(wm => wm.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(wm => wm.User)
            .WithMany(u => u.WorkspaceMemberships)
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(wm => wm.InvitedByUser)
            .WithMany()
            .HasForeignKey(wm => wm.InvitedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasIndex(wm => wm.UserId);
        
        builder.HasQueryFilter(wm => wm.User.DeletedAtUtc == null);
    }
}