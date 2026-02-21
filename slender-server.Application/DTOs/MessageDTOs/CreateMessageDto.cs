using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.MessageDTOs;

public sealed record CreateMessageDto
{
    public required string ChannelId { get; init; }
    public required string AuthorId { get; init; }
    public string? ReplyToId { get; init; }
    public required string Body { get; init; }
}

public static class CreateMessageDtoExtensions
{
    public static Message ToEntity(this CreateMessageDto dto)
    {
        return new Message
        {
            Id = Message.NewId(),
            ChannelId = dto.ChannelId,
            AuthorId = dto.AuthorId,
            ReplyToId = dto.ReplyToId,
            Body = dto.Body,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}
