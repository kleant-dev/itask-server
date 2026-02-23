using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IUserRepository
{
    Task<string?> GetIdByIdentityIdAsync(string identityId, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}