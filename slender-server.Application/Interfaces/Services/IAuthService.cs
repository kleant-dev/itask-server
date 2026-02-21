using slender_server.Application.DTOs.Auth;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AccessTokenDto>> RegisterAsync(RegisterUserDto registerUserDto, CancellationToken cancellationToken = default);
    Task<Result<AccessTokenDto>> LoginAsync(LoginUserDto loginUserDto, CancellationToken cancellationToken = default);
    Task<Result<AccessTokenDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<AccessTokenDto>> GoogleLoginAsync(string googleToken, CancellationToken cancellationToken = default);
}