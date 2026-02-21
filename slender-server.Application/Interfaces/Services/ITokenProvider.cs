using slender_server.Application.DTOs.Auth;

namespace slender_server.Application.Interfaces.Services;

public interface ITokenProvider
{
    /// <summary>
    /// Generates a pair of access and refresh tokens based on the provided request.
    /// </summary>
    AccessTokenDto Create(TokenRequest tokenRequest);
}