using System.Linq.Expressions;

namespace slender_server.Domain.Interfaces;

public interface ITaskRepository : IRepository<Domain.Entities.Task>
{
    Task<PagedResult<Domain.Entities.Task>> GetTasksByWorkspaceAsync<TDto>(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? status = null,
        string? sort = null,
        CancellationToken ct = default);
    
    Task<PagedResult<Domain.Entities.Task>> GetTasksByProjectAsync(
        string projectId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}