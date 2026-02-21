using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelMemberDTOs;

public sealed record CreateChannelMemberDto
{
    public required string ChannelId { get; init; }
    public string? UserId { get; init; }
}

public static class CreateChannelMemberDtoExtensions
{
    public static ChannelMember ToEntity(this CreateChannelMemberDto dto)
    {
        return new ChannelMember
        {
            ChannelId = dto.ChannelId,
            UserId = dto.UserId,
            JoinedAtUtc = DateTime.UtcNow
        };
    }
}
