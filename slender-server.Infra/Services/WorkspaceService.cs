// slender-server.Infra/Services/WorkspaceService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.DTOs.WorkspaceInviteDTOs;
using slender_server.Application.DTOs.WorkspaceMemberDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;

namespace slender_server.Infra.Services;

public sealed class WorkspaceService(
    IWorkspaceRepository workspaceRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IWorkspaceInviteRepository workspaceInviteRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    ISortingService sortingService,
    ILogger<WorkspaceService> logger)
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
            return Result<WorkspaceDto>.Failure($"A workspace with slug '{dto.Slug}' already exists");

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

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("slug") == true ||
                                           ex.InnerException?.Message.Contains("unique") == true)
        {
            return Result<WorkspaceDto>.Failure($"A workspace with slug '{dto.Slug}' already exists");
        }

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
        logger.LogInformation("Fetching workspaces for user {UserId}", userId);
        var query = workspaceRepository.Query()
            .Where(w => w.Members.Any(m => m.UserId == userId));
        logger.LogInformation("Query built for user {UserId}, found {Count} workspaces", userId, await query.CountAsync(cancellationToken));
        // Count before ordering — ORDER BY on a COUNT query is wasteful.
        var totalCount = await query.CountAsync(cancellationToken);

        // ApplySort<TDto, TEntity> resolves the registered WorkspaceSortMapping from DI.
        // SortParams.Sort is the sort string (e.g. "name asc, createdAt desc").
        query = sortingService.ApplySort<WorkspaceDto, Workspace>(
            query,
            sort.Sort,
            defaultOrderBy: nameof(Workspace.CreatedAtUtc) + " DESC");

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        // IPaginationService.MapToPagedResponse expects a PagedResult<TEntity>.
        var pagedResult = new PagedResult<Workspace>(items, totalCount, pagination.PageNumber, pagination.PageSize);
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
            return Result<WorkspaceDto>.Failure("Workspace not found");

        // Use already-loaded Members — avoids a redundant DB round-trip.
        var isMember = workspace.Members.Any(m => m.UserId == userId);
        if (!isMember)
            return Result<WorkspaceDto>.Failure("You do not have access to this workspace");

        return Result<WorkspaceDto>.Success(workspace.ToDto());
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
            return Result<WorkspaceDto>.Failure("Workspace not found");

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(role))
            return Result<WorkspaceDto>.Failure("You do not have permission to update this workspace");

        // ApplyTo patches only non-null fields — safe for partial updates.
        dto.ApplyTo(workspace);

        workspaceRepository.Update(workspace);
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
            return Result.Failure("Workspace not found");

        if (workspace.OwnerId != userId)
            return Result.Failure("Only the workspace owner can delete the workspace");

        workspace.DeletedAtUtc = DateTime.UtcNow;
        workspaceRepository.Update(workspace);
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
            return Result<PagedResponse<WorkspaceMemberDto>>.Failure("You do not have access to this workspace");

        var query = workspaceMemberRepository.Query()
            .Include(m => m.User)
            .Where(m => m.WorkspaceId == workspaceId);

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<WorkspaceRole>(role, true, out var roleEnum))
            query = query.Where(m => m.Role == roleEnum);

        // Count on the filtered query (before ordering) so total reflects the role filter.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.JoinedAtUtc)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<WorkspaceMember>(items, totalCount, pagination.PageNumber, pagination.PageSize);
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
            return Result<WorkspaceMemberDto>.Failure("You do not have permission to update member roles");

        // Validate role string first — fail fast before extra DB queries.
        if (!Enum.TryParse<WorkspaceRole>(newRole, true, out var roleEnum))
            return Result<WorkspaceMemberDto>.Failure("Invalid role");

        // Admins cannot elevate to Owner — compare enums, not strings.
        if (currentMemberRole == WorkspaceRole.Admin && roleEnum == WorkspaceRole.Owner)
            return Result<WorkspaceMemberDto>.Failure("Only the owner can assign the owner role");

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
            return Result<WorkspaceMemberDto>.Failure("Member not found");

        if (targetMember.Role == WorkspaceRole.Owner)
            return Result<WorkspaceMemberDto>.Failure("Cannot change the role of the workspace owner");

        targetMember.Role = roleEnum;
        workspaceMemberRepository.Update(targetMember);
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
            return Result.Failure("You do not have permission to remove members");

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
            return Result.Failure("Member not found");

        if (targetMember.Role == WorkspaceRole.Owner)
            return Result.Failure("Cannot remove the workspace owner");

        if (currentMemberRole == WorkspaceRole.Admin && targetMember.Role == WorkspaceRole.Admin)
            return Result.Failure("Admins cannot remove other admins");

        workspaceMemberRepository.Remove(targetMember);
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
            return Result<WorkspaceInviteDto>.Failure("Workspace not found");

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(role))
            return Result<WorkspaceInviteDto>.Failure("You do not have permission to invite members");

        var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
        {
            var alreadyMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, existingUser.Id, cancellationToken);
            if (alreadyMember)
                return Result<WorkspaceInviteDto>.Failure("User is already a member of this workspace");
        }

        var existingInvite = await workspaceInviteRepository.GetPendingInviteAsync(workspaceId, dto.Email, cancellationToken);
        if (existingInvite is not null)
            return Result<WorkspaceInviteDto>.Failure("An active invite already exists for this email");

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
            return Result<WorkspaceMemberDto>.Failure("Invite not found");

        if (invite.AcceptedAtUtc is not null)
            return Result<WorkspaceMemberDto>.Failure("Invite has already been accepted");

        if (invite.ExpiresAtUtc < DateTime.UtcNow)
            return Result<WorkspaceMemberDto>.Failure("Invite has expired");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<WorkspaceMemberDto>.Failure("User not found");

        if (user.Email != invite.Email)
            return Result<WorkspaceMemberDto>.Failure("This invite is for a different email address");

        var isMember = await workspaceMemberRepository.IsMemberAsync(invite.WorkspaceId, userId, cancellationToken);
        if (isMember)
            return Result<WorkspaceMemberDto>.Failure("You are already a member of this workspace");

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
        workspaceInviteRepository.Update(invite);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkspaceMemberDto>.Success(member.ToDto());
    }
}