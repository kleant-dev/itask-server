using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasMaxLength(50);
        
        builder.Property(m => m.ChannelId).HasMaxLength(50).IsRequired();
        builder.Property(m => m.AuthorId).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Body).HasColumnType("text").IsRequired();
        builder.Property(m => m.ReplyToId).HasMaxLength(50);
        
        builder.Property(m => m.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property(m => m.UpdatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        builder.Property(m => m.DeletedAtUtc);
        
        builder.HasQueryFilter(m => m.DeletedAtUtc == null);

        builder.HasOne(m=>m.Channel)
            .WithMany(c=>c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m=>m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(m=>m.ReplyToMessage)
            .WithMany(m=>m.Replies)
            .HasForeignKey(m => m.ReplyToId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(m => new { m.ChannelId, m.CreatedAtUtc })
            .HasDatabaseName("IX_Messages_Channel_Created")
            .IsDescending(false, true)
            .HasFilter("\"deleted_at_utc\" IS NULL");
        builder.HasIndex(m => new { m.AuthorId, m.CreatedAtUtc })
            .HasDatabaseName("IX_Messages_Author_Created")
            .IsDescending(false, true)
            .HasFilter("\"deleted_at_utc\" IS NULL");
        builder.HasIndex(m => m.ReplyToId)
            .HasFilter("\"reply_to_id\" IS NOT NULL AND \"deleted_at_utc\" IS NULL")
            .HasDatabaseName("IX_Messages_ReplyTo");
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Message_NoSelfReply",
                """
                "id" != "reply_to_id"
                """
            );
            t.HasCheckConstraint(
                "CK_Message_BodyNotEmpty",
                """LENGTH(TRIM("body")) > 0"""
            );
            t.HasCheckConstraint(
                "CK_Message_UpdatedAfterCreated",
                """
                "updated_at_utc" >= "created_at_utc"
                """
            );
        });
    }
}