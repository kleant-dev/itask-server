// Domain/Interfaces/IProjectRepository.cs
using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default);
    Task<bool> IsUserMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default);
    Task<string?> GetUserRoleAsync(string projectId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetTaskCountAsync(string projectId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedTaskCountAsync(string projectId, CancellationToken cancellationToken = default);
}