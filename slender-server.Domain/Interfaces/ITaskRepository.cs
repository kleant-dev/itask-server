// Domain/Interfaces/ITaskRepository.cs

using Task = slender_server.Domain.Entities.Task;

namespace slender_server.Domain.Interfaces;

public interface ITaskRepository : IRepository<Task>
{
    Task<Task?> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetByAssigneeIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetSubtasksAsync(string parentTaskId, CancellationToken cancellationToken = default);
    Task<int> GetCommentCountAsync(string taskId, CancellationToken cancellationToken = default);
    Task<int> GetAttachmentCountAsync(string taskId, CancellationToken cancellationToken = default);
}