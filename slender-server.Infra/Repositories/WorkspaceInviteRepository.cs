using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceInviteRepository : Repository<WorkspaceInvite>, IWorkspaceInviteRepository
{
    public WorkspaceInviteRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await DbSet
            .Include(wi => wi.Workspace)
            .Include(wi => wi.InvitedByUser)
            .FirstOrDefaultAsync(wi => wi.Token == token, ct);
    }

    public async Task<List<WorkspaceInvite>> GetPendingInvitesByWorkspaceAsync(
        string workspaceId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(wi => wi.WorkspaceId == workspaceId && wi.AcceptedAtUtc == null)
            .Where(wi => wi.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(wi => wi.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> HasPendingInviteAsync(
        string workspaceId,
        string email,
        CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(
            wi => wi.WorkspaceId == workspaceId &&
                  wi.Email == email &&
                  wi.AcceptedAtUtc == null &&
                  wi.ExpiresAtUtc > DateTime.UtcNow,
            ct);
    }

    public async Task<WorkspaceInvite?> GetPendingInviteAsync(string workspaceId, string email, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(wi => wi.WorkspaceId == workspaceId && wi.Email == email && wi.AcceptedAtUtc == null && wi.ExpiresAtUtc > DateTime.UtcNow, ct);
    }

    public void Update(WorkspaceInvite invite)
    {
        DbSet.Update(invite);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}