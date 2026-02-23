// Infra/Repositories/WorkspaceMemberRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceMemberRepository : Repository<WorkspaceMember>, IWorkspaceMemberRepository
{
    public WorkspaceMemberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkspaceMember>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.User)
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceMember?> GetMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(m => m.WorkspaceId == workspaceId, cancellationToken);
    }
}