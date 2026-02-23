using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceMemberRepository : IRepository<WorkspaceMember>
{
    Task<IEnumerable<WorkspaceMember>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(string workspaceId, CancellationToken cancellationToken = default);
}