using itask_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace itask_server.Infra.Database.Configurations;

public sealed class ProjectMembershipConfiguration : IEntityTypeConfiguration<ProjectMembership>
{
    public void Configure(EntityTypeBuilder<ProjectMembership> builder)
    {
        
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // builder.HasOne<Project>()
            //     .WithMany()
            //     .HasForeignKey(e => e.ProjectId)
            //     .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
    }
}