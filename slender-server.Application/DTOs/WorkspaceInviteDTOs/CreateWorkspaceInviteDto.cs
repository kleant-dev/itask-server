using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceInviteDTOs;

public sealed record CreateWorkspaceInviteDto
{
    public required string WorkspaceId { get; init; }
    public required string Email { get; init; }
    public required string InvitedByUserId { get; init; }
    public required string Token { get; init; }
    public required WorkspaceRole Role { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}

public static class CreateWorkspaceInviteDtoExtensions
{
    public static WorkspaceInvite ToEntity(this CreateWorkspaceInviteDto dto)
    {
        return new WorkspaceInvite
        {
            Id = WorkspaceInvite.NewId(),
            WorkspaceId = dto.WorkspaceId,
            Email = dto.Email,
            InvitedByUserId = dto.InvitedByUserId,
            Token = dto.Token,
            Role = dto.Role,
            ExpiresAtUtc = dto.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
