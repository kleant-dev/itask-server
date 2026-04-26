using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;

namespace slender_server.Application.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResponse<TaskDto>> GetWorkspaceTasksAsync(
        string workspaceId,
        PaginationParams paginationParams,
        string? sort = null,
        string? status = null,
        string? projectId = null,
        CancellationToken ct = default);

    Task<Result<TaskDto>> GetByIdAsync(
        string taskId,
        string userId,
        CancellationToken ct = default);

    Task<Result<TaskDto>> CreateTaskAsync(
        string userId,
        CreateTaskDto dto,
        CancellationToken ct = default);

    Task<Result<TaskDto>> UpdateTaskAsync(
        string taskId,
        string userId,
        UpdateTaskDto dto,
        CancellationToken ct = default);

    Task<Result> DeleteTaskAsync(
        string taskId,
        string userId,
        CancellationToken ct = default);
}

