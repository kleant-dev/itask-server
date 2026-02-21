namespace slender_server.Domain.Entities;

public sealed class Label
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }    // workspace-scoped, reusable
    public required string Name { get; set; }
    public required string Color { get; set; }          // hex color
    public DateTime CreatedAtUtc { get; set; }
    
    public Workspace Workspace { get; set; } = null!;
    public ICollection<TaskLabel> TaskLabels { get; set; } = [];
    
    public static string NewId() => $"l-{Guid.CreateVersion7()}";
}