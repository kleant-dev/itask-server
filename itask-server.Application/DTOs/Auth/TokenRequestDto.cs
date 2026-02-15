namespace itask_server.Application.DTOs.Auth;

public sealed record TokenRequest(string UserId, string Email, IEnumerable<string> Roles);