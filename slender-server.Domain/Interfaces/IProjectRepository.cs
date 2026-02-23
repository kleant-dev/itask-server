using slender_server.Domain.Entities;
using slender_server.Domain.Models;

namespace slender_server.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<PagedResult<Project>> GetProjectsByWorkspaceAsync(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken ct = default);
    
    Task<Project?> GetByIdWithDetailsAsync(
        string projectId,
        CancellationToken ct = default);
}