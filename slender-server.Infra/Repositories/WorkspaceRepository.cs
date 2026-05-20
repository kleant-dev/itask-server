using Microsoft.EntityFrameworkCore;
using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceRepository(ApplicationDbContext dbContext,ISortingService sortingService)
    : Repository<Workspace>(dbContext), IWorkspaceRepository
{
    public async Task<Workspace?> GetByIdWithMembersAsync(string workspaceId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
    }

    public async Task<PagedResult<Workspace>> GetUserWorkspacesAsync(string userId, int pageNumber, int pageSize, string? sort, CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(w => w.Members.Any(m => m.UserId == userId));
        sortingService.ApplySort<WorkspaceDto, Workspace>(
            query,
            sort,
            defaultOrderBy: nameof(Workspace.CreatedAtUtc) + " DESC");
        
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Workspace>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
    
    public async Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await DbSet
            .AnyAsync(w => w.Id == workspaceId && w.Members.Any(m => m.UserId == userId), ct);
    }

    public async Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(w => w.Slug == slug, ct);
    }

    /// <summary>
    /// Returns the workspace role of the user as a typed enum. Never returns a string.
    /// Callers should compare against WorkspaceRole enum values, not strings.
    /// </summary>
    public async Task<WorkspaceRole?> GetMemberRoleAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await _dbContext.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Select(m => (WorkspaceRole?)m.Role)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        // Note: application-level check only. The DB has a unique index on Slug
        // which is the real enforcement. Catch DbUpdateException in the service
        // to handle the rare race condition.
        return await _dbContext.Workspaces.AnyAsync(w => w.Slug == slug, ct);
    }

    public IQueryable<Workspace> Query()
    {
        return DbSet.AsQueryable();
    }

    public async Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<Workspace, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.CountAsync(predicate, ct);
    }

}