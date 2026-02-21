using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Infra.Database;
using TaskStatus = slender_server.Domain.Entities.TaskStatus;

namespace slender_server.Infra.Database.Seeders;

public static class ApplicationSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager)
    {
        // Don't seed if data already exists
        if (await db.Users.AnyAsync())
        {
            Console.WriteLine("⏭️  Database already seeded. Skipping...");
            return;
        }
        
        Console.WriteLine("🌱 Starting database seeding...");
        
        // 1. Bridge Identity → Application Users
        var users = await SeedUsersAsync(db, userManager);
        
        // 2. Seed Workspaces
        var workspace = await SeedWorkspaceAsync(db, users["alice"]);
        
        // 3. Seed Workspace Members
        await SeedWorkspaceMembersAsync(db, workspace.Id, users);
        
        // 4. Seed Projects
        var projects = await SeedProjectsAsync(db, workspace.Id, users["alice"]);
        
        // 5. Seed Project Members
        await SeedProjectMembersAsync(db, projects, users);
        
        // 6. Seed Labels
        var labels = await SeedLabelsAsync(db, workspace.Id);
        
        // 7. Seed Tasks
        var tasks = await SeedTasksAsync(db, workspace.Id, projects, users);
        
        // 8. Seed Task Assignees
        await SeedTaskAssigneesAsync(db, tasks, users);
        
        // 9. Seed Task Labels
        await SeedTaskLabelsAsync(db, tasks, labels);
        
        // 10. Seed Task Comments
        await SeedTaskCommentsAsync(db, tasks, users);
        
        // 11. Seed Calendar Events
        await SeedCalendarEventsAsync(db, workspace.Id, users["alice"], tasks);
        
        // 12. Seed Notifications
        await SeedNotificationsAsync(db, users, tasks);
        
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Database seeding completed!");
    }
    
    private static async Task<Dictionary<string, User>> SeedUsersAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager)
    {
        Console.WriteLine("👤 Seeding users...");
        
        var userDict = new Dictionary<string, User>();
        
        var identityUsers = await userManager.Users.ToListAsync();
        
        foreach (var identityUser in identityUsers)
        {
            var name = identityUser.Email!.Split('@')[0];
            var user = new User
            {
                Id = User.NewId(),
                IdentityId = identityUser.Id,
                Email = identityUser.Email!,
                Name = char.ToUpper(name[0]) + name[1..],
                DisplayName = char.ToUpper(name[0]) + name[1..],
                AvatarColor = GenerateRandomColor(),
                CreatedAtUtc = DateTime.UtcNow
            };
            
            db.Users.Add(user);
            userDict[name] = user;
        }
        
        await db.SaveChangesAsync();
        Console.WriteLine($"   ✅ Created {userDict.Count} users");
        return userDict;
    }
    
    private static async Task<Workspace> SeedWorkspaceAsync(
        ApplicationDbContext db,
        User owner)
    {
        Console.WriteLine("🏢 Seeding workspace...");
        
        var workspace = new Workspace
        {
            Id = Workspace.NewId(),
            OwnerId = owner.Id,
            Name = "Acme Corp",
            Slug = "acme-corp",
            Description = "Building the future of productivity",
            CreatedAtUtc = DateTime.UtcNow
        };
        
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();
        
        Console.WriteLine("   ✅ Created workspace: Acme Corp");
        return workspace;
    }
    
    private static async System.Threading.Tasks.Task SeedWorkspaceMembersAsync(
        ApplicationDbContext db,
        string workspaceId,
        Dictionary<string, User> users)
    {
        Console.WriteLine("👥 Seeding workspace members...");
        
        var members = new[]
        {
            new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = users["alice"].Id,
                Role = WorkspaceRole.Owner,
                JoinedAtUtc = DateTime.UtcNow
            },
            new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = users["bob"].Id,
                Role = WorkspaceRole.Admin,
                InvitedByUserId = users["alice"].Id,
                JoinedAtUtc = DateTime.UtcNow.AddDays(-5)
            },
            new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = users["charlie"].Id,
                Role = WorkspaceRole.Member,
                InvitedByUserId = users["alice"].Id,
                JoinedAtUtc = DateTime.UtcNow.AddDays(-3)
            }
        };
        
        db.WorkspaceMembers.AddRange(members);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Added {members.Length} workspace members");
    }
    
    private static async Task<Dictionary<string, Project>> SeedProjectsAsync(
        ApplicationDbContext db,
        string workspaceId,
        User owner)
    {
        Console.WriteLine("📁 Seeding projects...");
        
        var projects = new Dictionary<string, Project>
        {
            ["design-system"] = new Project
            {
                Id = Project.NewId(),
                WorkspaceId = workspaceId,
                OwnerId = owner.Id,
                Name = "Design System",
                Description = "Build a comprehensive design system for all products",
                Status = ProjectStatus.Active,
                Color = "#6366f1",
                Icon = "🎨",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                TargetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-30)
            },
            ["mobile-app"] = new Project
            {
                Id = Project.NewId(),
                WorkspaceId = workspaceId,
                OwnerId = owner.Id,
                Name = "Mobile App",
                Description = "Native mobile app for iOS and Android",
                Status = ProjectStatus.Active,
                Color = "#10b981",
                Icon = "📱",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                TargetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
            },
            ["marketing"] = new Project
            {
                Id = Project.NewId(),
                WorkspaceId = workspaceId,
                OwnerId = owner.Id,
                Name = "Marketing Campaign",
                Description = "Q1 2025 marketing initiatives",
                Status = ProjectStatus.Active,
                Color = "#f59e0b",
                Icon = "📢",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
            }
        };
        
        db.Projects.AddRange(projects.Values);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Created {projects.Count} projects");
        return projects;
    }
    
    private static async System.Threading.Tasks.Task SeedProjectMembersAsync(
        ApplicationDbContext db,
        Dictionary<string, Project> projects,
        Dictionary<string, User> users)
    {
        Console.WriteLine("🧑‍💼 Seeding project members...");
        
        var members = new List<ProjectMember>
        {
            // Design System
            new() { ProjectId = projects["design-system"].Id, UserId = users["alice"].Id, Role = ProjectMemberRole.Lead, JoinedAtUtc = DateTime.UtcNow.AddDays(-30) },
            new() { ProjectId = projects["design-system"].Id, UserId = users["bob"].Id, Role = ProjectMemberRole.Member, JoinedAtUtc = DateTime.UtcNow.AddDays(-28) },
            
            // Mobile App
            new() { ProjectId = projects["mobile-app"].Id, UserId = users["bob"].Id, Role = ProjectMemberRole.Lead, JoinedAtUtc = DateTime.UtcNow.AddDays(-15) },
            new() { ProjectId = projects["mobile-app"].Id, UserId = users["charlie"].Id, Role = ProjectMemberRole.Member, JoinedAtUtc = DateTime.UtcNow.AddDays(-14) },
            
            // Marketing
            new() { ProjectId = projects["marketing"].Id, UserId = users["charlie"].Id, Role = ProjectMemberRole.Lead, JoinedAtUtc = DateTime.UtcNow.AddDays(-7) },
            new() { ProjectId = projects["marketing"].Id, UserId = users["alice"].Id, Role = ProjectMemberRole.Viewer, JoinedAtUtc = DateTime.UtcNow.AddDays(-6) }
        };
        
        db.ProjectMembers.AddRange(members);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Added {members.Count} project members");
    }
    
    private static async Task<Dictionary<string, Label>> SeedLabelsAsync(
        ApplicationDbContext db,
        string workspaceId)
    {
        Console.WriteLine("🏷️  Seeding labels...");
        
        var labels = new Dictionary<string, Label>
        {
            ["bug"] = new Label { Id = Label.NewId(), WorkspaceId = workspaceId, Name = "Bug", Color = "#ef4444", CreatedAtUtc = DateTime.UtcNow },
            ["feature"] = new Label { Id = Label.NewId(), WorkspaceId = workspaceId, Name = "Feature", Color = "#3b82f6", CreatedAtUtc = DateTime.UtcNow },
            ["urgent"] = new Label { Id = Label.NewId(), WorkspaceId = workspaceId, Name = "Urgent", Color = "#f59e0b", CreatedAtUtc = DateTime.UtcNow },
            ["backend"] = new Label { Id = Label.NewId(), WorkspaceId = workspaceId, Name = "Backend", Color = "#8b5cf6", CreatedAtUtc = DateTime.UtcNow },
            ["frontend"] = new Label { Id = Label.NewId(), WorkspaceId = workspaceId, Name = "Frontend", Color = "#06b6d4", CreatedAtUtc = DateTime.UtcNow },
        };
        
        db.Labels.AddRange(labels.Values);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Created {labels.Count} labels");
        return labels;
    }
    
    private static async Task<Dictionary<string, Domain.Entities.Task>> SeedTasksAsync(
        ApplicationDbContext db,
        string workspaceId,
        Dictionary<string, Project> projects,
        Dictionary<string, User> users)
    {
        Console.WriteLine("✅ Seeding tasks...");
        
        var tasks = new Dictionary<string, Domain.Entities.Task>
        {
            ["task1"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["design-system"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["alice"].Id,
                Title = "Create color palette",
                Description = "Define primary, secondary, and neutral colors for the design system",
                Status = TaskStatus.Done,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(-5),
                SortOrder = 1000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-20),
                CompletedAtUtc = DateTime.UtcNow.AddDays(-10)
            },
            ["task2"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["design-system"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["alice"].Id,
                Title = "Design typography system",
                Description = "Define font families, sizes, weights, and line heights",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(5),
                ScheduledAt = DateTime.UtcNow.Date.AddHours(10),
                DurationMinutes = 120,
                SortOrder = 2000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
            },
            ["task3"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["design-system"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["bob"].Id,
                Title = "Build component library",
                Description = "Create reusable React components based on design tokens",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(15),
                SortOrder = 3000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
            },
            ["task4"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["mobile-app"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["bob"].Id,
                Title = "Implement authentication flow",
                Description = "OAuth 2.0 integration with Google and Apple Sign-In",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Urgent,
                DueDate = DateTime.UtcNow.AddDays(3),
                ScheduledAt = DateTime.UtcNow.Date.AddHours(14),
                DurationMinutes = 180,
                SortOrder = 1000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-8)
            },
            ["task5"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["mobile-app"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["charlie"].Id,
                Title = "Design onboarding screens",
                Description = "Create user-friendly onboarding experience with animations",
                Status = TaskStatus.Review,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
                SortOrder = 2000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-5)
            },
            ["task6"] = new()
            {
                Id = Domain.Entities.Task.NewId(),
                ProjectId = projects["marketing"].Id,
                WorkspaceId = workspaceId,
                CreatedById = users["charlie"].Id,
                Title = "Launch social media campaign",
                Description = "Coordinate posts across Twitter, LinkedIn, and Instagram",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(10),
                ScheduledAt = DateTime.UtcNow.AddDays(2).AddHours(9),
                DurationMinutes = 60,
                SortOrder = 1000,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
            }
        };
        
        db.Tasks.AddRange(tasks.Values);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Created {tasks.Count} tasks");
        return tasks;
    }
    
    private static async System.Threading.Tasks.Task SeedTaskAssigneesAsync(
        ApplicationDbContext db,
        Dictionary<string, Domain.Entities.Task> tasks,
        Dictionary<string, User> users)
    {
        Console.WriteLine("🙋 Seeding task assignees...");
        
        var assignees = new List<TaskAssignee>
        {
            new() { TaskId = tasks["task1"].Id, UserId = users["alice"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-20) },
            new() { TaskId = tasks["task2"].Id, UserId = users["alice"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-15) },
            new() { TaskId = tasks["task2"].Id, UserId = users["bob"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-14) },
            new() { TaskId = tasks["task3"].Id, UserId = users["bob"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-10) },
            new() { TaskId = tasks["task4"].Id, UserId = users["bob"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-8) },
            new() { TaskId = tasks["task4"].Id, UserId = users["charlie"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-7) },
            new() { TaskId = tasks["task5"].Id, UserId = users["charlie"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-5) },
            new() { TaskId = tasks["task6"].Id, UserId = users["charlie"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-3) },
            new() { TaskId = tasks["task6"].Id, UserId = users["alice"].Id, AssignedAtUtc = DateTime.UtcNow.AddDays(-2) },
        };
        
        db.TaskAssignees.AddRange(assignees);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Added {assignees.Count} task assignees");
    }
    
    private static async System.Threading.Tasks.Task SeedTaskLabelsAsync(
        ApplicationDbContext db,
        Dictionary<string, Domain.Entities.Task> tasks,
        Dictionary<string, Label> labels)
    {
        Console.WriteLine("🔗 Seeding task labels...");
        
        var taskLabels = new List<TaskLabel>
        {
            new() { TaskId = tasks["task1"].Id, LabelId = labels["feature"].Id },
            new() { TaskId = tasks["task2"].Id, LabelId = labels["feature"].Id },
            new() { TaskId = tasks["task2"].Id, LabelId = labels["frontend"].Id },
            new() { TaskId = tasks["task3"].Id, LabelId = labels["feature"].Id },
            new() { TaskId = tasks["task3"].Id, LabelId = labels["frontend"].Id },
            new() { TaskId = tasks["task4"].Id, LabelId = labels["feature"].Id },
            new() { TaskId = tasks["task4"].Id, LabelId = labels["backend"].Id },
            new() { TaskId = tasks["task4"].Id, LabelId = labels["urgent"].Id },
        };
        
        db.TaskLabels.AddRange(taskLabels);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Added {taskLabels.Count} task labels");
    }
    
    private static async System.Threading.Tasks.Task SeedTaskCommentsAsync(
        ApplicationDbContext db,
        Dictionary<string, Domain.Entities.Task> tasks,
        Dictionary<string, User> users)
    {
        Console.WriteLine("💬 Seeding task comments...");
        
        var comments = new List<TaskComment>
        {
            new()
            {
                Id = TaskComment.NewId(),
                TaskId = tasks["task2"].Id,
                AuthorId = users["bob"].Id,
                Body = "Looking great! I'll start on the component implementation once this is done.",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-12),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                Id = TaskComment.NewId(),
                TaskId = tasks["task4"].Id,
                AuthorId = users["charlie"].Id,
                Body = "Should we also add biometric authentication for future releases?",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-6),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = TaskComment.NewId(),
                TaskId = tasks["task5"].Id,
                AuthorId = users["bob"].Id,
                Body = "The animations look smooth! Can we add a skip button?",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-2)
            }
        };
        
        db.TaskComments.AddRange(comments);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Added {comments.Count} task comments");
    }
    
    private static async System.Threading.Tasks.Task SeedCalendarEventsAsync(
        ApplicationDbContext db,
        string workspaceId,
        User creator,
        Dictionary<string, Domain.Entities.Task> tasks)
    {
        Console.WriteLine("📅 Seeding calendar events...");
        
        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = CalendarEvent.NewId(),
                WorkspaceId = workspaceId,
                CreatedById = creator.Id,
                TaskId = tasks["task2"].Id,
                Title = "Design System Review",
                Description = "Review typography and spacing decisions",
                ScheduleType = "Meeting",
                StartsAtUtc = DateTime.UtcNow.Date.AddHours(10),
                EndsAtUtc = DateTime.UtcNow.Date.AddHours(11),
                IsAllDay = false,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = CalendarEvent.NewId(),
                WorkspaceId = workspaceId,
                CreatedById = creator.Id,
                TaskId = tasks["task4"].Id,
                Title = "Auth Implementation Sprint",
                Description = "Focus time for OAuth integration",
                ScheduleType = "Task",
                StartsAtUtc = DateTime.UtcNow.Date.AddHours(14),
                EndsAtUtc = DateTime.UtcNow.Date.AddHours(17),
                IsAllDay = false,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = CalendarEvent.NewId(),
                WorkspaceId = workspaceId,
                CreatedById = creator.Id,
                Title = "Team All-Hands",
                Description = "Monthly team sync and planning",
                ScheduleType = "Meeting",
                StartsAtUtc = DateTime.UtcNow.AddDays(3).Date.AddHours(15),
                EndsAtUtc = DateTime.UtcNow.AddDays(3).Date.AddHours(16),
                IsAllDay = false,
                CreatedAtUtc = DateTime.UtcNow
            }
        };
        
        db.CalendarEvents.AddRange(events);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Created {events.Count} calendar events");
    }
    
    private static async System.Threading.Tasks.Task SeedNotificationsAsync(
        ApplicationDbContext db,
        Dictionary<string, User> users,
        Dictionary<string, Domain.Entities.Task> tasks)
    {
        Console.WriteLine("🔔 Seeding notifications...");
        
        var notifications = new List<Notification>
        {
            new()
            {
                Id = Notification.NewId(),
                RecipientId = users["bob"].Id,
                ActorId = users["alice"].Id,
                Type = NotificationType.TaskAssigned,
                TaskId = tasks["task2"].Id,
                Title = "You were assigned to a task",
                EntityName = "Design typography system",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-14)
            },
            new()
            {
                Id = Notification.NewId(),
                RecipientId = users["alice"].Id,
                ActorId = users["bob"].Id,
                Type = NotificationType.Commented,
                TaskId = tasks["task2"].Id,
                Title = "Bob commented on a task",
                Body = "Looking great! I'll start on the component implementation...",
                EntityName = "Design typography system",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-12),
                ReadAtUtc = DateTime.UtcNow.AddDays(-11)
            },
            new()
            {
                Id = Notification.NewId(),
                RecipientId = users["bob"].Id,
                ActorId = null,
                Type = NotificationType.Deadline,
                TaskId = tasks["task4"].Id,
                Title = "Task deadline approaching",
                Body = "Implement authentication flow is due in 3 days",
                EntityName = "Implement authentication flow",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-6)
            }
        };
        
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"   ✅ Created {notifications.Count} notifications");
    }
    
    private static string GenerateRandomColor()
    {
        var random = new Random();
        return $"#{random.Next(0x1000000):X6}";
    }
}