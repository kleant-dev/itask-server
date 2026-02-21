using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.UserDTOs;

public sealed record CreateUserDto
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? AvatarUrl { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarColor { get; init; }
    public required string IdentityId { get; init; }
}

public static class CreateUserDtoExtensions
{
    public static User ToEntity(this CreateUserDto dto)
    {
        return new User
        {
            Id = User.NewId(),
            Email = dto.Email,
            Name = dto.Name,
            AvatarUrl = dto.AvatarUrl,
            DisplayName = dto.DisplayName,
            AvatarColor = dto.AvatarColor,
            IdentityId = dto.IdentityId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
