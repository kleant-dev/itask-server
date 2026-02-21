using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelDTOs;

public sealed record UpdateChannelDto
{
    public string? Name { get; init; }
    public string? ProjectId { get; init; }
}

public static class UpdateChannelDtoExtensions
{
    public static void ApplyTo(this UpdateChannelDto dto, Channel entity)
    {
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.ProjectId is not null) entity.ProjectId = dto.ProjectId;
    }
}
