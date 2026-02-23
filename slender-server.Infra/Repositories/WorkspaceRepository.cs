// Infra/Repositories/WorkspaceRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Workspace?> GetByIdWithMembersAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Owner)
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workspace?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(w => w.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Owner)
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }

    public async Task<WorkspaceMember?> GetMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }

    public async Task<string?> GetMemberRoleAsync(string workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
        
        return member?.Role.ToString();
    }
}