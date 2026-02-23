// Application/Services/UserService.cs

using slender_server.Application.DTOs.UserDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        
        if (user is null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            AvatarColor = user.AvatarColor,
            LastActiveAtUtc = user.LastActiveAtUtc,
            CreatedAtUtc = user.CreatedAtUtc
        };

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<UserDto>> UpdateCurrentUserAsync(
        string identityId, 
        UpdateUserDto updateUserDto, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        
        if (user is null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        user.Name = updateUserDto.Name;
        user.DisplayName = updateUserDto.DisplayName ?? user.DisplayName;
        user.AvatarUrl = updateUserDto.AvatarUrl ?? user.AvatarUrl;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _userRepository.(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            AvatarColor = user.AvatarColor,
            LastActiveAtUtc = user.LastActiveAtUtc,
            CreatedAtUtc = user.CreatedAtUtc
        };

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<string>> UploadAvatarAsync(
        string identityId, 
        Stream fileStream, 
        string fileName, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        
        if (user is null)
        {
            return Result<string>.Failure("User not found");
        }

        // TODO: Implement actual file upload to cloud storage
        var avatarUrl = $"https://cdn.slender.app/avatars/{user.Id}/{fileName}";
        
        user.AvatarUrl = avatarUrl;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(avatarUrl);
    }

    public async Task<Result> DeleteAccountAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure("User not found");
        }

        user.DeletedAtUtc = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateLastActiveAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure("User not found");
        }

        user.LastActiveAtUtc = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}