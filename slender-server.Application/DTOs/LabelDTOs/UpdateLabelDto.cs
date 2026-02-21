using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.LabelDTOs;

public sealed record UpdateLabelDto
{
    public string? Name { get; init; }
    public string? Color { get; init; }
}

public static class UpdateLabelDtoExtensions
{
    public static void ApplyTo(this UpdateLabelDto dto, Label entity)
    {
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Color is not null) entity.Color = dto.Color;
    }
}
