using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceInviteRepository : Repository<WorkspaceInvite>, IWorkspaceInviteRepository
{
    public WorkspaceInviteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Workspace)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<WorkspaceInvite?> GetPendingInviteAsync(string workspaceId, string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => 
                    i.WorkspaceId == workspaceId && 
                    i.Email == email && 
                    i.AcceptedAtUtc == null && 
                    i.ExpiresAtUtc > DateTime.UtcNow, 
                cancellationToken);
    }

    public async Task<IEnumerable<WorkspaceInvite>> GetPendingInvitesForEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Workspace)
            .Where(i => 
                i.Email == email && 
                i.AcceptedAtUtc == null && 
                i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}