using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.LabelDTOs;

public sealed record LabelDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class LabelDtoExtensions
{
    public static LabelDto ToDto(this Label entity)
    {
        return new LabelDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            Name = entity.Name,
            Color = entity.Color,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
