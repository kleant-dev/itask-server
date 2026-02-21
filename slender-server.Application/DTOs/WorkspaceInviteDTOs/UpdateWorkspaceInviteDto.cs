using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceInviteDTOs;

public sealed record UpdateWorkspaceInviteDto
{
    public WorkspaceRole? Role { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime? AcceptedAtUtc { get; init; }
}

public static class UpdateWorkspaceInviteDtoExtensions
{
    public static void ApplyTo(this UpdateWorkspaceInviteDto dto, WorkspaceInvite entity)
    {
        if (dto.Role is not null) entity.Role = dto.Role.Value;
        if (dto.ExpiresAtUtc is not null) entity.ExpiresAtUtc = dto.ExpiresAtUtc.Value;
        if (dto.AcceptedAtUtc is not null) entity.AcceptedAtUtc = dto.AcceptedAtUtc;
    }
}
