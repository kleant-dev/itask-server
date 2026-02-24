// Infra/Repositories/ProjectRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Workspace)
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Owner)
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
    }

    public async Task<string?> GetUserRoleAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
        
        return member?.Role.ToString();
    }

    public async Task<int> GetTaskCountAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .CountAsync(t => t.ProjectId == projectId, cancellationToken);
    }

    public async Task<int> GetCompletedTaskCountAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .CountAsync(t => t.ProjectId == projectId && t.CompletedAtUtc != null, cancellationToken);
    }
}