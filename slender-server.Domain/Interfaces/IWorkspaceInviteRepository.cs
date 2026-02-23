using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceInviteRepository : IRepository<WorkspaceInvite>
{
    Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<WorkspaceInvite?> GetPendingInviteAsync(string workspaceId, string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkspaceInvite>> GetPendingInvitesForEmailAsync(string email, CancellationToken cancellationToken = default);
}