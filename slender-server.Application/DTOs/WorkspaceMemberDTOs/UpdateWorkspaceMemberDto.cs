using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.WorkspaceMemberDTOs;

public sealed record UpdateWorkspaceMemberDto
{
    public WorkspaceRole? Role { get; init; }
}

public static class UpdateWorkspaceMemberDtoExtensions
{
    public static void ApplyTo(this UpdateWorkspaceMemberDto dto, WorkspaceMember entity)
    {
        if (dto.Role is not null) entity.Role = dto.Role.Value;
    }
}
