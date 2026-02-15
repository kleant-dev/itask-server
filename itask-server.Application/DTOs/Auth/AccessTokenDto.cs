namespace itask_server.Application.DTOs.Auth;

public sealed record AccessTokenDto(string AccessToken, string RefreshToken);