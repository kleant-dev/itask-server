using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelMemberDTOs;

public sealed record UpdateChannelMemberDto
{
    public DateTime? LastReadAtUtc { get; init; }
}

public static class UpdateChannelMemberDtoExtensions
{
    public static void ApplyTo(this UpdateChannelMemberDto dto, ChannelMember entity)
    {
        if (dto.LastReadAtUtc is not null) entity.LastReadAtUtc = dto.LastReadAtUtc;
    }
}
