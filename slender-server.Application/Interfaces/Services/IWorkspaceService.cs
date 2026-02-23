using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.DTOs.WorkspaceMemberDTOs;
using slender_server.Application.DTOs.WorkspaceInviteDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.Interfaces.Services;

public interface IWorkspaceService
{
    Task<Result<WorkspaceDto>> CreateWorkspaceAsync(string userId, CreateWorkspaceDto dto, CancellationToken cancellationToken = default);
    Task<Result<PagedResponse<WorkspaceDto>>> GetUserWorkspacesAsync(string userId, PaginationParams pagination, SortParams sort, string? fields, CancellationToken cancellationToken = default);
    Task<Result<WorkspaceDto>> GetWorkspaceByIdAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    Task<Result<WorkspaceDto>> UpdateWorkspaceAsync(string workspaceId, string userId, UpdateWorkspaceDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkspaceAsync(string workspaceId, string userId, CancellationToken cancellationToken = default);
    
    // Members
    Task<Result<PagedResponse<WorkspaceMemberDto>>> GetWorkspaceMembersAsync(string workspaceId, string userId, PaginationParams pagination, SortParams sort, string? role, CancellationToken cancellationToken = default);
    Task<Result<WorkspaceMemberDto>> UpdateMemberRoleAsync(string workspaceId, string userId, string targetUserId, string newRole, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(string workspaceId, string userId, string targetUserId, CancellationToken cancellationToken = default);
    
    // Invites
    Task<Result<WorkspaceInviteDto>> InviteUserAsync(string workspaceId, string userId, CreateWorkspaceInviteDto dto, CancellationToken cancellationToken = default);
    Task<Result<WorkspaceMemberDto>> AcceptInviteAsync(string token, string userId, CancellationToken cancellationToken = default);
}