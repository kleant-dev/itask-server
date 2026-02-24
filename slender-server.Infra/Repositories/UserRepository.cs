using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Infra.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public async Task<string?> GetIdByIdentityIdAsync(string identityId, CancellationToken ct)
    {
        return await DbSet
            .Where(u => u.IdentityId == identityId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(u => u.Id == id, cancellationToken);
    }
}