using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ProjectMemberDTOs;

public sealed record ProjectMemberDto
{
    public required string ProjectId { get; init; }
    public required string UserId { get; init; }
    public required ProjectMemberRole Role { get; init; }
    public string? AddedByUserId { get; init; }
    public DateTime JoinedAtUtc { get; init; }
}

public static class ProjectMemberDtoExtensions
{
    public static ProjectMemberDto ToDto(this ProjectMember entity)
    {
        return new ProjectMemberDto
        {
            ProjectId = entity.ProjectId,
            UserId = entity.UserId,
            Role = entity.Role,
            AddedByUserId = entity.AddedByUserId,
            JoinedAtUtc = entity.JoinedAtUtc
        };
    }
}
