using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.Auth;

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
            UpdatedAtUtc = DateTime.UtcNow,
            AvatarColor = GenerateRandomColor(),
            LastActiveAtUtc = DateTime.UtcNow,
            DisplayName = registerUserDto.Name
        };
    }
    private static string GenerateRandomColor()
    {
        var random = new Random();
        return $"#{random.Next(0x1000000):X6}";
    }
}