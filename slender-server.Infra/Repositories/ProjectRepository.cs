using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class ProjectRepository(ApplicationDbContext dbContext)
    : Repository<Project>(dbContext), IProjectRepository
{
    public async Task<PagedResult<Project>> GetProjectsByWorkspaceAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        return await GetPagedAsync(
            pageNumber,
            pageSize,
            filter: p => p.WorkspaceId == workspaceId,
            orderBy: q => q.OrderByDescending(p => p.CreatedAtUtc),
            ct: ct);
    }
}