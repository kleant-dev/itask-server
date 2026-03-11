using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceInviteRepository : IRepository<WorkspaceInvite>
{
    Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<WorkspaceInvite>> GetPendingInvitesByWorkspaceAsync(string workspaceId, CancellationToken ct = default);
    Task<bool> HasPendingInviteAsync(string workspaceId, string email, CancellationToken ct = default);

    /// <summary>
    /// Returns a pending (not yet accepted, not expired) invite for the given workspace + email.
    /// </summary>
    Task<WorkspaceInvite?> GetPendingInviteAsync(string workspaceId, string email, CancellationToken ct = default);

    void Update(WorkspaceInvite invite);
}