using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<Workspace?> GetByIdWithMembersAsync(string workspaceId, CancellationToken ct = default);
    Task<List<Workspace>> GetUserWorkspacesAsync(string userId, CancellationToken ct = default);
    Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken ct = default);
    Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default);
}