
using slender_server.Application.DTOs.UserDTOs;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetCurrentUserAsync(string identityId, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateCurrentUserAsync(string identityId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task<Result<string>> UploadAvatarAsync(string identityId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Result> DeleteAccountAsync(string identityId, CancellationToken cancellationToken = default);
    Task<Result> UpdateLastActiveAsync(string identityId, CancellationToken cancellationToken = default);
}