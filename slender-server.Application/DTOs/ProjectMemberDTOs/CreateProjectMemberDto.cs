using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectMemberDTOs;

public sealed record CreateProjectMemberDto
{
    public required string ProjectId { get; init; }
    public required string UserId { get; init; }
    public required ProjectMemberRole Role { get; init; }
    public string? AddedByUserId { get; init; }
}

public static class CreateProjectMemberDtoExtensions
{
    public static ProjectMember ToEntity(this CreateProjectMemberDto dto)
    {
        return new ProjectMember
        {
            ProjectId = dto.ProjectId,
            UserId = dto.UserId,
            Role = dto.Role,
            AddedByUserId = dto.AddedByUserId,
            JoinedAtUtc = DateTime.UtcNow
        };
    }
}
