using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.MessageDTOs;

public sealed record MessageDto
{
    public required string Id { get; init; }
    public required string ChannelId { get; init; }
    public required string AuthorId { get; init; }
    public string? ReplyToId { get; init; }
    public required string Body { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}

public static class MessageDtoExtensions
{
    public static MessageDto ToDto(this Message entity)
    {
        return new MessageDto
        {
            Id = entity.Id,
            ChannelId = entity.ChannelId,
            AuthorId = entity.AuthorId,
            ReplyToId = entity.ReplyToId,
            Body = entity.Body,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
