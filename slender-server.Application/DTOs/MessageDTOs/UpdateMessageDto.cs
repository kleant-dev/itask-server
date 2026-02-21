using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.MessageDTOs;

public sealed record UpdateMessageDto
{
    public string? Body { get; init; }
}

public static class UpdateMessageDtoExtensions
{
    public static void ApplyTo(this UpdateMessageDto dto, Message entity)
    {
        if (dto.Body is not null) entity.Body = dto.Body;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
