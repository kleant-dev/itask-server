using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.UserDTOs;

public sealed record UpdateUserDto
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? AvatarUrl { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarColor { get; init; }
}

public static class UpdateUserDtoExtensions
{
    public static void ApplyTo(this UpdateUserDto dto, User entity)
    {
        if (dto.Email is not null) entity.Email = dto.Email;
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.AvatarUrl is not null) entity.AvatarUrl = dto.AvatarUrl;
        if (dto.DisplayName is not null) entity.DisplayName = dto.DisplayName;
        if (dto.AvatarColor is not null) entity.AvatarColor = dto.AvatarColor;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
