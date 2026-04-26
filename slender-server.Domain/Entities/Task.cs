namespace slender_server.Domain.Entities;

public sealed class Task
{
    public required string Id { get; set; }
    public required string ProjectId { get; set; }
    public required string WorkspaceId { get; set; }    // DENORMALIZED - critical for StatsBar/MyTasks
    public string? CreatedById { get; set; }
    public string? ParentTaskId { get; set; }           // subtasks (1 level only)
    
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.None;
    
    public DateTime? DueDate { get; set; }
    public DateTime? ScheduledAt { get; set; }          // Powers Schedule widget
    public int? DurationMinutes { get; set; }
    
    public double SortOrder { get; set; }               // float for drag-and-drop
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }         // soft delete
    
    public Project Project { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public User? CreatedBy { get; set; }
    public Task? ParentTask { get; set; }
    public ICollection<Task> Subtasks { get; set; } = [];
    public ICollection<TaskAssignee> Assignees { get; set; } = [];
    public ICollection<TaskComment> Comments { get; set; } = [];
    public ICollection<TaskAttachment> Attachments { get; set; } = [];
    public ICollection<TaskLabel> Labels { get; set; } = [];
    
    public static string NewId() => $"t-{Guid.CreateVersion7()}";
}

public enum TaskStatus
{
    Todo,           
    InProgress,    
    InReview,
    Done,
    Archived
    // NOTE: "Overdue" is COMPUTED, not stored - see below
}

public enum TaskPriority
{
    None,
    Low,
    Medium,
    High,
    Urgent
}