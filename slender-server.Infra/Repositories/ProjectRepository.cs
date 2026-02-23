using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Project>> GetProjectsByWorkspaceAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Where(p => p.WorkspaceId == workspaceId)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status.ToString() == status);
        }

        // Get total count
        int totalCount = await query.CountAsync(ct);

        // Apply pagination with ordering
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Project>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Project?> GetByIdWithDetailsAsync(
        string projectId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);
    }
}