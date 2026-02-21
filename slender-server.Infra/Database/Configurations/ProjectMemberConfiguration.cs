using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });
        
        builder.Property(pm => pm.ProjectId).HasMaxLength(50);
        builder.Property(pm => pm.UserId).HasMaxLength(50);
        builder.Property(pm => pm.AddedByUserId).HasMaxLength(50).IsRequired(false);
        builder.Property(pm => pm.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(pm => pm.JoinedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(pm => pm.AddedByUser)
            .WithMany()
            .HasForeignKey(pm => pm.AddedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasIndex(pm => pm.UserId);
        
        builder.HasQueryFilter(pm => pm.User.DeletedAtUtc == null);
    }
}