// Infra/Repositories/TaskRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class TaskRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.Task>(context), ITaskRepository
{
    public async Task<Domain.Entities.Task?> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Include(t => t.Labels)
                .ThenInclude(tl => tl.Label)
            .Include(t => t.ParentTask)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetByAssigneeIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .Where(t => t.Assignees.Any(a => a.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetSubtasksAsync(string parentTaskId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Where(t => t.ParentTaskId == parentTaskId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCommentCountAsync(string taskId, CancellationToken cancellationToken = default)
    {
        return await context.TaskComments
            .CountAsync(c => c.TaskId == taskId, cancellationToken);
    }

    public async Task<int> GetAttachmentCountAsync(string taskId, CancellationToken cancellationToken = default)
    {
        return await context.TaskAttachments
            .CountAsync(a => a.TaskId == taskId, cancellationToken);
    }
}