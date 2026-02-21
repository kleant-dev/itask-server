using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectMemberDTOs;

public sealed record UpdateProjectMemberDto
{
    public ProjectMemberRole? Role { get; init; }
}

public static class UpdateProjectMemberDtoExtensions
{
    public static void ApplyTo(this UpdateProjectMemberDto dto, ProjectMember entity)
    {
        if (dto.Role is not null) entity.Role = dto.Role.Value;
    }
}
