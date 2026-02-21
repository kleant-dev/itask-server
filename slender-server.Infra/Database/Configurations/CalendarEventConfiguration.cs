using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace slender_server.Infra.Database.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.HasKey(ce => ce.Id);
        builder.Property(ce => ce.Id).HasMaxLength(50);
        
        builder.Property(ce => ce.WorkspaceId).HasMaxLength(50).IsRequired();
        builder.Property(ce => ce.CreatedById).HasMaxLength(50).IsRequired();
        builder.Property(ce => ce.Title).HasMaxLength(255).IsRequired();
        builder.Property(ce => ce.StartsAtUtc).IsRequired();
        builder.Property(ce => ce.EndsAtUtc).IsRequired();
        builder.Property(ce => ce.IsAllDay).IsRequired();
        builder.Property(ce => ce.TaskId).HasMaxLength(50);
        builder.Property(ce => ce.Description).HasColumnType("text");
        builder.Property(ce => ce.ScheduleType).HasMaxLength(50);
        
        builder.Property(ce => ce.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(ce => ce.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ce => ce.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<slender_server.Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(ce => ce.TaskId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(ce => new { ce.WorkspaceId, ce.StartsAtUtc })
            .HasDatabaseName("IX_CalendarEvents_Workspace_StartsAt");
        builder.HasIndex(ce => new { ce.CreatedById, ce.StartsAtUtc })
            .HasDatabaseName("IX_CalendarEvents_Creator_StartsAt");
        builder.HasIndex(ce => ce.TaskId)
            .HasFilter("\"task_id\" IS NOT NULL")
            .HasDatabaseName("IX_CalendarEvents_Task");
        builder.HasIndex(ce => new { ce.WorkspaceId, ce.StartsAtUtc, ce.EndsAtUtc })
            .HasDatabaseName("IX_CalendarEvents_Workspace_DateRange");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_CalendarEvent_EndsAfterStarts",
                """
                "ends_at_utc" > "starts_at_utc"
                """
            );
            t.HasCheckConstraint(
                "CK_CalendarEvent_AllDayMidnight",
                """
                ("is_all_day" = false) OR 
                          ("is_all_day" = true AND 
                           EXTRACT(HOUR FROM "starts_at_utc") = 0 AND 
                           EXTRACT(MINUTE FROM "starts_at_utc") = 0 AND 
                           EXTRACT(SECOND FROM "starts_at_utc") = 0 AND
                           EXTRACT(HOUR FROM "ends_at_utc") = 0 AND 
                           EXTRACT(MINUTE FROM "ends_at_utc") = 0 AND 
                           EXTRACT(SECOND FROM "ends_at_utc") = 0)
                """
            );
            
            t.HasCheckConstraint(
                "CK_CalendarEvent_TitleNotEmpty",
                """LENGTH(TRIM("title")) > 0"""
            );
            
            t.HasCheckConstraint(
                "CK_CalendarEvent_ValidScheduleType",
                """
                "schedule_type" IS NULL OR 
                          "schedule_type" IN ('Meeting', 'Task', 'Review', 'Event', 'Reminder')
                """
            );
        });
    }
}