using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<Workspace?> GetByIdWithMembersAsync(string id, CancellationToken cancellationToken = default);
    Task<Workspace?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetMemberAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    Task<string?> GetMemberRoleAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
}