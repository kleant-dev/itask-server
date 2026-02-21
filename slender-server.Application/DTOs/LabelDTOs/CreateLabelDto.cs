using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.LabelDTOs;

public sealed record CreateLabelDto
{
    public required string WorkspaceId { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
}

public static class CreateLabelDtoExtensions
{
    public static Label ToEntity(this CreateLabelDto dto)
    {
        return new Label
        {
            Id = Label.NewId(),
            WorkspaceId = dto.WorkspaceId,
            Name = dto.Name,
            Color = dto.Color,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
