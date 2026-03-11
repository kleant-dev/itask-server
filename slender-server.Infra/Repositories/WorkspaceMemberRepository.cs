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
        return await DbSet
            .Include(wm => wm.User)
            .Include(wm => wm.Workspace)
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, ct);
    }

    public async Task<PagedResult<WorkspaceMember>> GetWorkspaceMembersAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? role = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<WorkspaceRole>(role, true, out var roleEnum))
        {
            query = query.Where(wm => wm.Role == roleEnum);
        }

        int totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(wm => wm.JoinedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkspaceMember>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> IsUserInWorkspaceAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, ct);
    }

    public async Task<WorkspaceRole?> GetUserRoleAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId)
            .Select(wm => (WorkspaceRole?)wm.Role)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsMemberAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, ct);
    }

    public async Task<WorkspaceMember?> GetMemberAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(wm => wm.User)
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, ct);
    }

    public IQueryable<WorkspaceMember> Query()
    {
        return DbSet.AsQueryable();
    }

    public async Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<WorkspaceMember, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.CountAsync(predicate, ct);
    }

}