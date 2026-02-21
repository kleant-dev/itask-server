using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceDTOs;

public sealed record UpdateWorkspaceDto
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
}

public static class UpdateWorkspaceDtoExtensions
{
    public static void ApplyTo(this UpdateWorkspaceDto dto, Workspace entity)
    {
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Slug is not null) entity.Slug = dto.Slug;
        if (dto.LogoUrl is not null) entity.LogoUrl = dto.LogoUrl;
        if (dto.Description is not null) entity.Description = dto.Description;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
