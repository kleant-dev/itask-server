using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelMemberDTOs;

public sealed record ChannelMemberDto
{
    public required string ChannelId { get; init; }
    public string? UserId { get; init; }
    public DateTime? LastReadAtUtc { get; init; }
    public DateTime JoinedAtUtc { get; init; }
}

public static class ChannelMemberDtoExtensions
{
    public static ChannelMemberDto ToDto(this ChannelMember entity)
    {
        return new ChannelMemberDto
        {
            ChannelId = entity.ChannelId,
            UserId = entity.UserId,
            LastReadAtUtc = entity.LastReadAtUtc,
            JoinedAtUtc = entity.JoinedAtUtc
        };
    }
}
