using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<PagedResult<Project>> GetProjectsByWorkspaceAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}