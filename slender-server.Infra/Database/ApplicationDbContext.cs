using slender_server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Interfaces;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options),IUnitOfWork
{
    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<WorkspaceInvite> WorkspaceInvites { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskLabel> TaskLabels { get; set; }
    public DbSet<TaskAssignee> TaskAssignees { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ChannelMember> ChannelMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TaskCommentAttachment> TaskCommentAttachments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Soft-delete global query filter: deleted workspaces are invisible to all queries.
        // WorkspaceConfiguration already guards on owner.DeletedAtUtc == null;
        // we add the self-filter here so SlugExistsAsync, GetUserWorkspacesAsync, etc.
        // automatically exclude soft-deleted workspaces.
        modelBuilder.Entity<Workspace>()
            .HasQueryFilter(w => w.DeletedAtUtc == null && w.Owner.DeletedAtUtc == null);
    }
    
}