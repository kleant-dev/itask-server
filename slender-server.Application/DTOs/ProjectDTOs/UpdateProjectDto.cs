using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectDTOs;

public sealed record UpdateProjectDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public ProjectStatus? Status { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? TargetDate { get; init; }
    public DateTime? ArchivedAtUtc { get; init; }
}

public static class UpdateProjectDtoExtensions
{
    public static void ApplyTo(this UpdateProjectDto dto, Project entity)
    {
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.Status is not null) entity.Status = dto.Status.Value;
        if (dto.Color is not null) entity.Color = dto.Color;
        if (dto.Icon is not null) entity.Icon = dto.Icon;
        if (dto.StartDate is not null) entity.StartDate = dto.StartDate;
        if (dto.TargetDate is not null) entity.TargetDate = dto.TargetDate;
        if (dto.ArchivedAtUtc is not null) entity.ArchivedAtUtc = dto.ArchivedAtUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
