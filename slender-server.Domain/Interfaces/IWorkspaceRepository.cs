using slender_server.Domain.Entities;
using slender_server.Domain.Models;

namespace slender_server.Domain.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<Workspace?> GetByIdWithMembersAsync(string workspaceId, CancellationToken ct = default);
    Task<PagedResult<Workspace>> GetUserWorkspacesAsync(
        string userId,
        int pageNumber,
        int pageSize,
        string? sort,
        CancellationToken ct = default);
    Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken ct = default);
    Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    IQueryable<Workspace> Query();

    /// <summary>
    /// Returns the workspace role of the given user, or null if not a member.
    /// Returns a typed enum — never compare as string.
    /// </summary>
    Task<WorkspaceRole?> GetMemberRoleAsync(string workspaceId, string userId, CancellationToken ct = default);

    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Workspace, bool>> predicate, CancellationToken ct = default);
}