using Microsoft.EntityFrameworkCore;
using slender_server.Application.Interfaces.Services;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class TaskRepository : Repository<Domain.Entities.Task>, ITaskRepository
{
    private readonly ISortingService _sortingService;

    public TaskRepository(
        ApplicationDbContext dbContext,
        ISortingService sortingService) : base(dbContext)
    {
        _sortingService = sortingService;
    }

    public async Task<PagedResult<Domain.Entities.Task>> GetTasksByWorkspaceAsync<TDto>(
        string workspaceId,
        int pageNumber,
        int pageSize,
        string? sort = null,
        string? status = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Where(t => t.WorkspaceId == workspaceId)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        // Apply sorting
        query = _sortingService.ApplySort<TDto, Domain.Entities.Task>(
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

        return new PagedResult<Domain.Entities.Task>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<Domain.Entities.Task>> GetTasksByProjectAsync(
        string projectId,
        int pageNumber,
        int pageSize,
        string? sort = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Where(t => t.ProjectId == projectId)
            .AsQueryable();

        // Apply sorting (use sortOrder by default for project tasks)
        query = string.IsNullOrWhiteSpace(sort)
            ? query.OrderBy(t => t.SortOrder)
            : _sortingService.ApplySort<Domain.Entities.Task, Domain.Entities.Task>(
                query,
                sort,
                defaultOrderBy: "SortOrder");

        // Get total count
        int totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Domain.Entities.Task>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Domain.Entities.Task>> GetUserTasksAsync(
        string userId,
        string? workspaceId = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Where(t => t.Assignees.Any(a => a.UserId == userId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            query = query.Where(t => t.WorkspaceId == workspaceId);
        }

        return await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }
}