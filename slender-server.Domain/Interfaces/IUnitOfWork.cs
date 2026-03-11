namespace slender_server.Domain.Interfaces;

/// <summary>
/// Unit of Work abstraction. Repositories track changes; call SaveChangesAsync once
/// to commit all changes in a single atomic transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}