using slender_server.Domain.Models;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Domain.Interfaces;

public interface ITaskRepository : IRepository<Task>
{
    Task<PagedResult<Task>> GetTasksByWorkspaceAsync<TDto>(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? sort = null,
        string? status = null,
        CancellationToken ct = default);
    
    Task<PagedResult<Task>> GetTasksByProjectAsync(
        string projectId,
        int pageNumber,
        int pageSize,
        string? sort = null,
        CancellationToken ct = default);
    
    Task<List<Task>> GetUserTasksAsync(
        string userId,
        string? workspaceId = null,
        CancellationToken ct = default);
}