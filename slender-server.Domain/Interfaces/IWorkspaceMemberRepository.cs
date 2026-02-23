using slender_server.Domain.Entities;
using slender_server.Domain.Models;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceMemberRepository : IRepository<WorkspaceMember>
{
    Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(string workspaceId, string userId, CancellationToken ct = default);
    Task<PagedResult<WorkspaceMember>> GetWorkspaceMembersAsync(string workspaceId, int pageNumber, int pageSize, string? role = null, CancellationToken ct = default);
    Task<bool> IsUserInWorkspaceAsync(string workspaceId, string userId, CancellationToken ct = default);
    Task<WorkspaceRole?> GetUserRoleAsync(string workspaceId, string userId, CancellationToken ct = default);
}