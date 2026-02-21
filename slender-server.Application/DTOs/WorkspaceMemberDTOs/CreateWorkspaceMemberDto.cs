using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceMemberDTOs;

public sealed record CreateWorkspaceMemberDto
{
    public required string WorkspaceId { get; init; }
    public required string UserId { get; init; }
    public required WorkspaceRole Role { get; init; }
    public string? InvitedByUserId { get; init; }
}

public static class CreateWorkspaceMemberDtoExtensions
{
    public static WorkspaceMember ToEntity(this CreateWorkspaceMemberDto dto)
    {
        return new WorkspaceMember
        {
            WorkspaceId = dto.WorkspaceId,
            UserId = dto.UserId,
            Role = dto.Role,
            InvitedByUserId = dto.InvitedByUserId,
            JoinedAtUtc = DateTime.UtcNow
        };
    }
}
