using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectDTOs;

public sealed record ProjectDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required ProjectStatus Status { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? TargetDate { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? ArchivedAtUtc { get; init; }
}

public static class ProjectDtoExtensions
{
    public static ProjectDto ToDto(this Project entity)
    {
        return new ProjectDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            OwnerId = entity.OwnerId,
            Name = entity.Name,
            Description = entity.Description,
            Status = entity.Status,
            Color = entity.Color,
            Icon = entity.Icon,
            StartDate = entity.StartDate,
            TargetDate = entity.TargetDate,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            ArchivedAtUtc = entity.ArchivedAtUtc
        };
    }
}
