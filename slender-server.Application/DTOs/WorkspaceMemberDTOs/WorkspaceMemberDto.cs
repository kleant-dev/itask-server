using slender_server.Domain.Entities;
using slender_server.Application.DTOs.UserDTOs;

namespace slender_server.Application.DTOs.WorkspaceMemberDTOs;

public sealed record WorkspaceMemberDto
{
    public required string WorkspaceId { get; init; }
    public required string UserId { get; init; }
    public required WorkspaceRole Role { get; init; }
    public string? InvitedByUserId { get; init; }
    public DateTime JoinedAtUtc { get; init; }
    public UserDto? User { get; init; }
}

public static class WorkspaceMemberDtoExtensions
{
    public static WorkspaceMemberDto ToDto(this WorkspaceMember entity)
    {
        return new WorkspaceMemberDto
        {
            WorkspaceId = entity.WorkspaceId,
            UserId = entity.UserId,
            Role = entity.Role,
            InvitedByUserId = entity.InvitedByUserId,
            JoinedAtUtc = entity.JoinedAtUtc,
            User = entity.User is not null ? entity.User.ToDto() : null
        };
    }
}
