using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelDTOs;

public sealed record ChannelDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public string? CreatedById { get; init; }
    public required ChannelType Type { get; init; }
    public string? Name { get; init; }
    public string? ProjectId { get; init; }
    public string? ParticipantHash { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class ChannelDtoExtensions
{
    public static ChannelDto ToDto(this Channel entity)
    {
        return new ChannelDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            CreatedById = entity.CreatedById,
            Type = entity.Type,
            Name = entity.Name,
            ProjectId = entity.ProjectId,
            ParticipantHash = entity.ParticipantHash,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
