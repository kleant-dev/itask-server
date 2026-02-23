using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceMemberRepository : Repository<WorkspaceMember>, IWorkspaceMemberRepository
{
    public WorkspaceMemberRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Include(wm => wm.User)
            .Include(wm => wm.Workspace)
            .FirstOrDefaultAsync(
                wm => wm.WorkspaceId == workspaceId && wm.UserId == userId,
                ct);
    }

    public async Task<PagedResult<WorkspaceMember>> GetWorkspaceMembersAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? role = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId)
            .AsQueryable();

        // Filter by role if specified
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<WorkspaceRole>(role, true, out var roleEnum))
        {
            query = query.Where(wm => wm.Role == roleEnum);
        }

        // Get total count
        int totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .OrderBy(wm => wm.JoinedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkspaceMember>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> IsUserInWorkspaceAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            wm => wm.WorkspaceId == workspaceId && wm.UserId == userId,
            ct);
    }

    public async Task<WorkspaceRole?> GetUserRoleAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default)
    {
        var member = await _dbSet
            .Where(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId)
            .Select(wm => (WorkspaceRole?)wm.Role)
            .FirstOrDefaultAsync(ct);

        return member;
    }
}