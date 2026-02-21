using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectDTOs;

public sealed record CreateProjectDto
{
    public required string WorkspaceId { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required ProjectStatus Status { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? TargetDate { get; init; }
}

public static class CreateProjectDtoExtensions
{
    public static Project ToEntity(this CreateProjectDto dto)
    {
        return new Project
        {
            Id = Project.NewId(),
            WorkspaceId = dto.WorkspaceId,
            OwnerId = dto.OwnerId,
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            Color = dto.Color,
            Icon = dto.Icon,
            StartDate = dto.StartDate,
            TargetDate = dto.TargetDate,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
