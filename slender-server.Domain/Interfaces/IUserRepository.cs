namespace slender_server.Domain.Interfaces;

public interface IUserRepository
{
    Task<string?> GetIdByIdentityIdAsync(string identityId, CancellationToken ct);
}