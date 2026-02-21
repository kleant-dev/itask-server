namespace slender_server.Domain.Entities;

public sealed class TaskLabel
{
    public required string TaskId { get; set; }
    public required string LabelId { get; set; }
    
    public Task Task { get; set; } = null!;
    public Label Label { get; set; } = null!;
    
}