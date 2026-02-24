using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceRepository(ApplicationDbContext dbContext)
    : Repository<Workspace>(dbContext), IWorkspaceRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Workspace?> GetByIdWithMembersAsync(string workspaceId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(w => w.Members)
            .ThenInclude(m => m.User)
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
    }

    public async Task<List<Workspace>> GetUserWorkspacesAsync(string userId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync(ct);
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

    public async Task<string?> GetMemberRoleAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        var member = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, ct);
        return member?.Role.ToString().ToLower();
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        var workspace = await _dbContext.Workspaces.SingleOrDefaultAsync(w => w.Slug == slug, cancellationToken: ct);
        return workspace is not null;
    }

    public IQueryable<Workspace> Query()
    {
        return DbSet.AsQueryable();
    }

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Workspace, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.CountAsync(predicate, ct);
    }

    public void Update(Workspace workspace)
    {
        DbSet.Update(workspace);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}