using itask_server.Domain.Entities;

namespace itask_server.Application.DTOs.Auth;

public sealed record RegisterUserDto
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public static class RegisterUserDtoExtensions
{
    public static User ToEntity(this RegisterUserDto registerUserDto)
    {
        return new User
        {
            Id = User.NewId(),
            Email = registerUserDto.Email,
            Name = registerUserDto.Name,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}