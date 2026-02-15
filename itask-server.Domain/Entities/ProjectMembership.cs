namespace itask_server.Domain.Entities;

public class ProjectMembership
{
    public Guid Id { get; set; } 
    public Guid ProjectId { get; set; } 
    public string UserId { get; set; } 
    public string Role { get; set; } 
}
