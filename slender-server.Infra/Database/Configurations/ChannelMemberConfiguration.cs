using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class ChannelMemberConfiguration : IEntityTypeConfiguration<ChannelMember>
{
    public void Configure(EntityTypeBuilder<ChannelMember> builder)
    {
        builder.HasKey(cm => new { cm.ChannelId, cm.UserId });
        
        builder.Property(cm => cm.ChannelId).HasMaxLength(50).IsRequired();
        builder.Property(cm => cm.UserId).HasMaxLength(50);
        builder.Property(cm => cm.LastReadAtUtc);
        builder.Property(cm => cm.JoinedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne(cm=>cm.Channel)
            .WithMany(c=>c.Members)
            .HasForeignKey(cm => cm.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasIndex(cm => cm.UserId);
        builder.HasIndex(cm => cm.UserId)
            .HasFilter("\"last_read_at_utc\" IS NULL")
            .HasDatabaseName("IX_ChannelMembers_UserId_Unread");
    }
}