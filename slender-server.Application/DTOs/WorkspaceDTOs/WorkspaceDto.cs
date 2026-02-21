using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceDTOs;

public sealed record WorkspaceDto
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public static class WorkspaceDtoExtensions
{
    public static WorkspaceDto ToDto(this Workspace entity)
    {
        return new WorkspaceDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Name = entity.Name,
            Slug = entity.Slug,
            LogoUrl = entity.LogoUrl,
            Description = entity.Description,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
