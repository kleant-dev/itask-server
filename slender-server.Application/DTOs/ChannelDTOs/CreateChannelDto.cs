using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelDTOs;

public sealed record CreateChannelDto
{
    public required string WorkspaceId { get; init; }
    public string? CreatedById { get; init; }
    public required ChannelType Type { get; init; }
    public string? Name { get; init; }
    public string? ProjectId { get; init; }
    public string? ParticipantHash { get; init; }
}

public static class CreateChannelDtoExtensions
{
    public static Channel ToEntity(this CreateChannelDto dto)
    {
        return new Channel
        {
            Id = Channel.NewId(),
            WorkspaceId = dto.WorkspaceId,
            CreatedById = dto.CreatedById,
            Type = dto.Type,
            Name = dto.Name,
            ProjectId = dto.ProjectId,
            ParticipantHash = dto.ParticipantHash,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
