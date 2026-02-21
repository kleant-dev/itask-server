namespace slender_server.Application.Interfaces.Services;

public interface IUserContext
{
    Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default);
    Task<string> GetRequiredUserIdAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}