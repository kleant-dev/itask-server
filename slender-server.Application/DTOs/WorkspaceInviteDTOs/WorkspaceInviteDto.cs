using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceInviteDTOs;

public sealed record WorkspaceInviteDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string Email { get; init; }
    public required string InvitedByUserId { get; init; }
    public required WorkspaceRole Role { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public DateTime? AcceptedAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class WorkspaceInviteDtoExtensions
{
    public static WorkspaceInviteDto ToDto(this WorkspaceInvite entity)
    {
        return new WorkspaceInviteDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            Email = entity.Email,
            InvitedByUserId = entity.InvitedByUserId,
            Role = entity.Role,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            AcceptedAtUtc = entity.AcceptedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
