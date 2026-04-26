using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork,
    ISortingService sortingService,
    IPaginationService paginationService,
    IUserContext userContext)
    : ITaskService
{
    public async Task<PagedResponse<TaskDto>> GetWorkspaceTasksAsync(
        string workspaceId,
        PaginationParams paginationParams,
        string? sort = null,
        string? status = null,
        string? projectId = null,
        CancellationToken ct = default)
    {
        if (!sortingService.ValidateSort<TaskDto, Task>(sort))
            throw new ArgumentException("Invalid sort fields", nameof(sort));

        _ = await userContext.GetRequiredUserIdAsync(ct);

        var pagedResult = await taskRepository.GetPagedAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize,
            t => t.WorkspaceId == workspaceId && (status == null || t.Status.ToString() == status) && (projectId == null || t.ProjectId == projectId),
            null,
            ct);

        return paginationService.MapToPagedResponse(pagedResult, t => t.ToDto());
    }

    public async Task<Result<TaskDto>> GetByIdAsync(string taskId, string userId, CancellationToken ct = default)
    {
        var task = await taskRepository.GetByIdWithDetailsAsync(taskId, ct);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(task.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<TaskDto>.Failure("You do not have access to this task",ErrorType.Forbidden);

        return Result<TaskDto>.Success(task.ToDto());
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(string userId, CreateTaskDto dto, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(dto.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<TaskDto>.Failure("You do not have permission to create tasks in this workspace",ErrorType.Forbidden);

        var entity = (dto with { CreatedById = userId }).ToEntity();
        await taskRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<TaskDto>.Success(entity.ToDto());
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(string taskId, string userId, UpdateTaskDto dto, CancellationToken ct = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, ct);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(task.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<TaskDto>.Failure("You do not have permission to update this task",ErrorType.Forbidden);

        dto.ApplyTo(task);
        await taskRepository.UpdateAsync(task, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<TaskDto>.Success(task.ToDto());
    }

    public async Task<Result> DeleteTaskAsync(string taskId, string userId, CancellationToken ct = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, ct);
        if (task is null)
            return Result.Failure("Task not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(task.WorkspaceId, userId, ct);
        if (!isMember)
            return Result.Failure("You do not have permission to delete this task",ErrorType.Forbidden);

        task.DeletedAtUtc = DateTime.UtcNow;
        await taskRepository.UpdateAsync(task, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
