// slender-server.Application/Services/TaskService.cs
using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Interfaces;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Application.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    ISortingService sortingService,
    IPaginationService paginationService,
    IUserContext userContext)
{
    public async Task<PagedResponse<TaskDto>> GetWorkspaceTasksAsync(
        string workspaceId,
        PaginationParams paginationParams,
        string? sort = null,
        string? status = null,
        CancellationToken ct = default)
    {
        if (!sortingService.ValidateSort<TaskDto, Domain.Entities.Task>(sort))
        {
            throw new ArgumentException("Invalid sort fields", nameof(sort));
        }

        string userId = await userContext.GetRequiredUserIdAsync(ct);
        
        var pagedResult = await taskRepository.GetPagedAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize,
            t => t.WorkspaceId == workspaceId && (status == null || t.Status.ToString() == status),
            null,
            ct);
        
        return paginationService.MapToPagedResponse(pagedResult, MapToDto);
    }
    
    private static TaskDto MapToDto(Task task)
    {
        return new TaskDto()
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status,
            Priority = task.Priority,
            ProjectId = task.ProjectId,
            WorkspaceId = task.WorkspaceId,
            DueDate = task.DueDate,
            CreatedAtUtc = task.CreatedAtUtc
        };
    }
}