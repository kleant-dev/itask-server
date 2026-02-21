using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.UserDTOs;

/// <summary>
/// GET (single or collection item) response for User.
/// </summary>
public sealed record UserDto
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? AvatarUrl { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarColor { get; init; }
    public DateTime? LastActiveAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public static class UserDtoExtensions
{
    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Email = entity.Email,
            Name = entity.Name,
            AvatarUrl = entity.AvatarUrl,
            DisplayName = entity.DisplayName,
            AvatarColor = entity.AvatarColor,
            LastActiveAtUtc = entity.LastActiveAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
