// slender-server.Infra/Services/WorkspaceService.cs

using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.DTOs.WorkspaceInviteDTOs;
using slender_server.Application.DTOs.WorkspaceMemberDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Application.Services;

public sealed class WorkspaceService(
    IWorkspaceRepository workspaceRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IWorkspaceInviteRepository workspaceInviteRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService)
    : IWorkspaceService
{
    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static bool IsPrivileged(WorkspaceRole? role) =>
        role is WorkspaceRole.Owner or WorkspaceRole.Admin;

    // ─── Create ─────────────────────────────────────────────────────────────────

    public async Task<Result<WorkspaceDto>> CreateWorkspaceAsync(
        string userId,
        CreateWorkspaceDto dto,
        CancellationToken cancellationToken = default)
    {
        var slugExists = await workspaceRepository.SlugExistsAsync(dto.Slug, cancellationToken);
        if (slugExists)
            return Result<WorkspaceDto>.Failure($"A workspace with slug '{dto.Slug}' already exists",ErrorType.Conflict);

        var workspace = new Workspace
        {
            Id = Workspace.NewId(),
            OwnerId = userId,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            LogoUrl = dto.LogoUrl,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var ownerMember = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = userId,
            Role = WorkspaceRole.Owner,
            JoinedAtUtc = DateTime.UtcNow,
            InvitedByUserId = null
        };

        await workspaceRepository.AddAsync(workspace, cancellationToken);
        await workspaceMemberRepository.AddAsync(ownerMember, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceDto>.Success(workspace.ToDto());
    }

    // ─── Read ────────────────────────────────────────────────────────────────────

    public async Task<Result<PagedResponse<WorkspaceDto>>> GetUserWorkspacesAsync(
        string userId,
        PaginationParams pagination,
        SortParams sort,
        string? fields,
        CancellationToken cancellationToken = default)
    {
        var query = workspaceRepository.Query()
            .Where(w => w.Members.Any(m => m.UserId == userId));

        var pagedResult = await workspaceRepository.GetUserWorkspacesAsync(
            userId,
            pagination.PageNumber,
            pagination.PageSize,
            sort.Sort,
            cancellationToken);
        // IPaginationService.MapToPagedResponse expects a PagedResult<TEntity>.
        var pagedResponse = paginationService.MapToPagedResponse(pagedResult, w => w.ToDto());

        return Result<PagedResponse<WorkspaceDto>>.Success(pagedResponse);
    }

    public async Task<Result<WorkspaceDto>> GetWorkspaceByIdAsync(
        string workspaceId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdWithMembersAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return Result<WorkspaceDto>.Failure("Workspace not found",ErrorType.NotFound);

        // Use already-loaded Members — avoids a redundant DB round-trip.
        var isMember = workspace.Members.Any(m => m.UserId == userId);
        return !isMember ? Result<WorkspaceDto>.Failure("You do not have access to this workspace",ErrorType.Forbidden) : Result<WorkspaceDto>.Success(workspace.ToDto());
    }

    // ─── Update ──────────────────────────────────────────────────────────────────

    public async Task<Result<WorkspaceDto>> UpdateWorkspaceAsync(
        string workspaceId,
        string userId,
        UpdateWorkspaceDto dto,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return Result<WorkspaceDto>.Failure("Workspace not found",ErrorType.NotFound);

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(role))
            return Result<WorkspaceDto>.Failure("You do not have permission to update this workspace",ErrorType.Forbidden);

        // ApplyTo patches only non-null fields — safe for partial updates.
        dto.ApplyTo(workspace);

        await workspaceRepository.UpdateAsync(workspace, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceDto>.Success(workspace.ToDto());
    }

    // ─── Delete ──────────────────────────────────────────────────────────────────

    public async Task<Result> DeleteWorkspaceAsync(
        string workspaceId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return Result.Failure("Workspace not found",ErrorType.NotFound);

        if (workspace.OwnerId != userId)
            return Result.Failure("Only the workspace owner can delete the workspace",ErrorType.Forbidden);

        workspace.DeletedAtUtc = DateTime.UtcNow;
        await workspaceRepository.UpdateAsync(workspace, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // ─── Members ─────────────────────────────────────────────────────────────────

    public async Task<Result<PagedResponse<WorkspaceMemberDto>>> GetWorkspaceMembersAsync(
        string workspaceId,
        string userId,
        PaginationParams pagination,
        SortParams sort,
        string? role,
        CancellationToken cancellationToken = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, cancellationToken);
        if (!isMember)
        {
            return Result<PagedResponse<WorkspaceMemberDto>>.Failure("You do not have access to this workspace",ErrorType.Forbidden);
        }

        var pagedResult = await workspaceMemberRepository
            .GetWorkspaceMembersAsync(
                workspaceId,
                pagination.PageNumber,
                pagination.PageSize,
                role,
                cancellationToken);

        var pagedResponse = paginationService.MapToPagedResponse(pagedResult, m => m.ToDto());

        return Result<PagedResponse<WorkspaceMemberDto>>.Success(pagedResponse);
    }

    public async Task<Result<WorkspaceMemberDto>> UpdateMemberRoleAsync(
        string workspaceId,
        string userId,
        string targetUserId,
        string newRole,
        CancellationToken cancellationToken = default)
    {
        var currentMemberRole = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(currentMemberRole))
            return Result<WorkspaceMemberDto>.Failure("You do not have permission to update member roles",ErrorType.Forbidden);

        // Validate role string first — fail fast before extra DB queries.
        if (!Enum.TryParse<WorkspaceRole>(newRole, true, out var roleEnum))
            return Result<WorkspaceMemberDto>.Failure("Invalid role",ErrorType.Validation);

        // Admins cannot elevate to Owner — compare enums, not strings.
        if (currentMemberRole == WorkspaceRole.Admin && roleEnum == WorkspaceRole.Owner)
            return Result<WorkspaceMemberDto>.Failure("Only the owner can assign the owner role",ErrorType.Forbidden);

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
            return Result<WorkspaceMemberDto>.Failure("Member not found",ErrorType.NotFound);

        if (targetMember.Role == WorkspaceRole.Owner)
            return Result<WorkspaceMemberDto>.Failure("Cannot change the role of the workspace owner",ErrorType.Forbidden);

        targetMember.Role = roleEnum;
        await workspaceMemberRepository.UpdateAsync(targetMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceMemberDto>.Success(targetMember.ToDto());
    }

    public async Task<Result> RemoveMemberAsync(
        string workspaceId,
        string userId,
        string targetUserId,
        CancellationToken cancellationToken = default)
    {
        var currentMemberRole = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(currentMemberRole))
            return Result.Failure("You do not have permission to remove members",ErrorType.Forbidden);

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
            return Result.Failure("Member not found",ErrorType.NotFound);

        if (targetMember.Role == WorkspaceRole.Owner)
            return Result.Failure("Cannot remove the workspace owner",ErrorType.Forbidden);

        if (currentMemberRole == WorkspaceRole.Admin && targetMember.Role == WorkspaceRole.Admin)
            return Result.Failure("Admins cannot remove other admins",ErrorType.Forbidden);

        await workspaceMemberRepository.DeleteAsync(targetMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // ─── Invites ─────────────────────────────────────────────────────────────────

    public async Task<Result<WorkspaceInviteDto>> InviteUserAsync(
        string workspaceId,
        string userId,
        CreateWorkspaceInviteDto dto,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return Result<WorkspaceInviteDto>.Failure("Workspace not found",ErrorType.NotFound);

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(role))
            return Result<WorkspaceInviteDto>.Failure("You do not have permission to invite members",ErrorType.Forbidden);

        var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
        {
            var alreadyMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, existingUser.Id, cancellationToken);
            if (alreadyMember)
                return Result<WorkspaceInviteDto>.Failure("User is already a member of this workspace",ErrorType.Conflict);
        }

        var existingInvite = await workspaceInviteRepository.GetPendingInviteAsync(workspaceId, dto.Email, cancellationToken);
        if (existingInvite is not null)
            return Result<WorkspaceInviteDto>.Failure("An active invite already exists for this email",ErrorType.Conflict);

        var invite = new WorkspaceInvite
        {
            Id = WorkspaceInvite.NewId(),
            WorkspaceId = workspaceId,
            Email = dto.Email,
            Role = dto.Role,
            Token = Guid.NewGuid().ToString("N"),
            InvitedByUserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow
        };

        await workspaceInviteRepository.AddAsync(invite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceInviteDto>.Success(new WorkspaceInviteDto
        {
            Id = invite.Id,
            WorkspaceId = invite.WorkspaceId,
            Email = invite.Email,
            Role = invite.Role,
            Token = invite.Token,
            InvitedByUserId = invite.InvitedByUserId,
            ExpiresAtUtc = invite.ExpiresAtUtc,
            CreatedAtUtc = invite.CreatedAtUtc,
            InviteUrl = $"https://app.slender.app/accept-invite?token={invite.Token}"
        });
    }

    public async Task<Result<WorkspaceMemberDto>> AcceptInviteAsync(
        string token,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var invite = await workspaceInviteRepository.GetByTokenAsync(token, cancellationToken);
        if (invite is null)
            return Result<WorkspaceMemberDto>.Failure("Invite not found",ErrorType.NotFound);

        if (invite.AcceptedAtUtc is not null)
            return Result<WorkspaceMemberDto>.Failure("Invite has already been accepted",ErrorType.Conflict);

        if (invite.ExpiresAtUtc < DateTime.UtcNow)
            return Result<WorkspaceMemberDto>.Failure("Invite has expired",ErrorType.Validation);

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<WorkspaceMemberDto>.Failure("User not found",ErrorType.NotFound);

        if (user.Email != invite.Email)
            return Result<WorkspaceMemberDto>.Failure("This invite is for a different email address",ErrorType.Conflict);

        var isMember = await workspaceMemberRepository.IsMemberAsync(invite.WorkspaceId, userId, cancellationToken);
        if (isMember)
            return Result<WorkspaceMemberDto>.Failure("You are already a member of this workspace",ErrorType.Conflict);

        var member = new WorkspaceMember
        {
            WorkspaceId = invite.WorkspaceId,
            UserId = userId,
            Role = invite.Role,
            JoinedAtUtc = DateTime.UtcNow,
            InvitedByUserId = invite.InvitedByUserId
        };

        // Atomic: both writes committed together — invite cannot be reused if member insert fails.
        await workspaceMemberRepository.AddAsync(member, cancellationToken);
        invite.AcceptedAtUtc = DateTime.UtcNow;
        await workspaceInviteRepository.UpdateAsync(invite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceMemberDto>.Success(member.ToDto());
    }
}