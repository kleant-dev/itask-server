using Microsoft.EntityFrameworkCore;
using slender_server.Application.Interfaces.Services;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;
using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Infra.Repositories;

public sealed class TaskRepository(ApplicationDbContext dbContext, ISortingService sortingService) : Repository<Task>(dbContext), ITaskRepository
{
    
    public async Task<PagedResult<Task>> GetTasksByWorkspaceAsync<TDto>(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? sort = null,
        string? status = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.Where(t => t.WorkspaceId == workspaceId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        // Apply sorting
        query = sortingService.ApplySort<TDto, Task>(
            query, 
            sort, 
            defaultOrderBy: "CreatedAtUtc desc");

        // Get total count
        int totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Task>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
    

    public async Task<PagedResult<Task>> GetTasksByProjectAsync(
        string projectId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        return await GetPagedAsync(
            pageNumber,
            pageSize,
            filter: t => t.ProjectId == projectId,
            orderBy: q => q.OrderBy(t => t.SortOrder),
            ct: ct);
    }
}