using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceDTOs;

public sealed record CreateWorkspaceDto
{
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
}

public static class CreateWorkspaceDtoExtensions
{
    public static Workspace ToEntity(this CreateWorkspaceDto dto)
    {
        return new Workspace
        {
            Id = Workspace.NewId(),
            OwnerId = dto.OwnerId,
            Name = dto.Name,
            Slug = dto.Slug,
            LogoUrl = dto.LogoUrl,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
