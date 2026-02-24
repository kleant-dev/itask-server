// Infra/Repositories/ProjectMemberRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class ProjectMemberRepository : Repository<ProjectMember>, IProjectMemberRepository
{
    public ProjectMemberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectMember?> GetMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(m => m.ProjectId == projectId, cancellationToken);
    }
}