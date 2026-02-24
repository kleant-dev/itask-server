// Application/Services/WorkspaceService.cs

using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.DTOs.WorkspaceInviteDTOs;
using slender_server.Application.DTOs.WorkspaceMemberDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;
using slender_server.Application.SortMappings;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class WorkspaceService(
    IWorkspaceRepository workspaceRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IWorkspaceInviteRepository workspaceInviteRepository,
    IUserRepository userRepository,
    IPaginationService paginationService,
    ISortingService sortingService)
    : IWorkspaceService
{
    public async Task<Result<WorkspaceDto>> CreateWorkspaceAsync(
        string userId, 
        CreateWorkspaceDto dto, 
        CancellationToken cancellationToken = default)
    {
        var slugExists = await workspaceRepository.SlugExistsAsync(dto.Slug, cancellationToken);
        if (slugExists)
        {
            return Result<WorkspaceDto>.Failure($"A workspace with slug '{dto.Slug}' already exists");
        }

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

        await workspaceRepository.AddAsync(workspace, cancellationToken);

        var ownerMember = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = userId,
            Role = WorkspaceRole.Owner,
            JoinedAtUtc = DateTime.UtcNow,
            InvitedByUserId = null
        };

        await workspaceMemberRepository.AddAsync(ownerMember, cancellationToken);

        var workspaceDto = new WorkspaceDto
        {
            Id = workspace.Id,
            OwnerId = workspace.OwnerId,
            Name = workspace.Name,
            Slug = workspace.Slug,
            Description = workspace.Description,
            LogoUrl = workspace.LogoUrl,
            CreatedAtUtc = workspace.CreatedAtUtc,
            UpdatedAtUtc = workspace.UpdatedAtUtc
        };

        return Result<WorkspaceDto>.Success(workspaceDto);
    }

    public async Task<Result<PagedResponse<WorkspaceDto>>> GetUserWorkspacesAsync(
        string userId,
        PaginationParams pagination,
        SortParams sort,
        string? fields,
        CancellationToken cancellationToken = default)
    {
        var query = workspaceRepository.Query()
            .Where(w => w.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrEmpty(sort.OrderBy))
        {
            query = sortingService.ApplySort(query, sort.OrderBy, new WorkspaceSortMapping());
        }
        else
        {
            query = query.OrderByDescending(w => w.CreatedAtUtc);
        }

        var totalCount = await workspaceRepository.CountAsync(w => w.Members.Any(m => m.UserId == userId), cancellationToken);

        var workspaces = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var workspaceDtos = workspaces.Select(w => new WorkspaceDto
        {
            Id = w.Id,
            OwnerId = w.OwnerId,
            Name = w.Name,
            Slug = w.Slug,
            Description = w.Description,
            LogoUrl = w.LogoUrl,
            CreatedAtUtc = w.CreatedAtUtc,
            UpdatedAtUtc = w.UpdatedAtUtc
        }).ToList();

        var pagedResponse = paginationService.CreatePagedResponse(
            workspaceDtos,
            pagination,
            totalCount);

        return Result<PagedResponse<WorkspaceDto>>.Success(pagedResponse);
    }

    public async Task<Result<WorkspaceDto>> GetWorkspaceByIdAsync(
        string workspaceId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdWithMembersAsync(workspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result<WorkspaceDto>.Failure("Workspace not found");
        }

        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, cancellationToken);
        if (!isMember)
        {
            return Result<WorkspaceDto>.Failure("You do not have access to this workspace");
        }

        var workspaceDto = new WorkspaceDto
        {
            Id = workspace.Id,
            OwnerId = workspace.OwnerId,
            Name = workspace.Name,
            Slug = workspace.Slug,
            Description = workspace.Description,
            LogoUrl = workspace.LogoUrl,
            CreatedAtUtc = workspace.CreatedAtUtc,
            UpdatedAtUtc = workspace.UpdatedAtUtc
        };

        return Result<WorkspaceDto>.Success(workspaceDto);
    }

    public async Task<Result<WorkspaceDto>> UpdateWorkspaceAsync(
        string workspaceId,
        string userId,
        UpdateWorkspaceDto dto,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result<WorkspaceDto>.Failure("Workspace not found");
        }

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (role is null || (role != "owner" && role != "admin"))
        {
            return Result<WorkspaceDto>.Failure("You do not have permission to update this workspace");
        }

        workspace.Name = dto.Name;
        workspace.Description = dto.Description;
        workspace.LogoUrl = dto.LogoUrl;
        workspace.UpdatedAtUtc = DateTime.UtcNow;

        workspaceRepository.Update(workspace);
        await workspaceRepository.SaveChangesAsync(cancellationToken);

        var workspaceDto = new WorkspaceDto
        {
            Id = workspace.Id,
            OwnerId = workspace.OwnerId,
            Name = workspace.Name,
            Slug = workspace.Slug,
            Description = workspace.Description,
            LogoUrl = workspace.LogoUrl,
            CreatedAtUtc = workspace.CreatedAtUtc,
            UpdatedAtUtc = workspace.UpdatedAtUtc
        };

        return Result<WorkspaceDto>.Success(workspaceDto);
    }

    public async Task<Result> DeleteWorkspaceAsync(
        string workspaceId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure("Workspace not found");
        }

        if (workspace.OwnerId != userId)
        {
            return Result.Failure("Only the workspace owner can delete the workspace");
        }

        workspace.DeletedAtUtc = DateTime.UtcNow;
        
        workspaceRepository.Update(workspace);
        await workspaceRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

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
            return Result<PagedResponse<WorkspaceMemberDto>>.Failure("You do not have access to this workspace");
        }

        var query = workspaceMemberRepository.Query()
            .Include(m => m.User)
            .Where(m => m.WorkspaceId == workspaceId);

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<WorkspaceRole>(role, true, out var roleEnum))
        {
            query = query.Where(m => m.Role == roleEnum);
        }

        query = query.OrderBy(m => m.JoinedAtUtc);

        var totalCount = await workspaceMemberRepository.CountAsync(m => m.WorkspaceId == workspaceId, cancellationToken);

        var members = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var memberDtos = members.Select(m => new WorkspaceMemberDto
        {
            UserId = m.UserId,
            WorkspaceId = m.WorkspaceId,
            Role = m.Role,
            JoinedAtUtc = m.JoinedAtUtc,
            InvitedByUserId = m.InvitedByUserId
        }).ToList();

        var pagedResponse = paginationService.CreatePagedResponse(
            memberDtos,
            pagination,
            totalCount);

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
        if (currentMemberRole is null || (currentMemberRole != "owner" && currentMemberRole != "admin"))
        {
            return Result<WorkspaceMemberDto>.Failure("You do not have permission to update member roles");
        }

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
        {
            return Result<WorkspaceMemberDto>.Failure("Member not found");
        }

        if (targetMember.Role == WorkspaceRole.Owner)
        {
            return Result<WorkspaceMemberDto>.Failure("Cannot change the role of the workspace owner");
        }

        if (currentMemberRole == "admin" && newRole == "owner")
        {
            return Result<WorkspaceMemberDto>.Failure("Only the owner can assign the owner role");
        }

        if (Enum.TryParse<WorkspaceRole>(newRole, true, out var roleEnum))
        {
            targetMember.Role = roleEnum;
        }
        else
        {
            return Result<WorkspaceMemberDto>.Failure("Invalid role");
        }
        
        workspaceMemberRepository.Update(targetMember);
        await workspaceMemberRepository.SaveChangesAsync(cancellationToken);

        var memberDto = new WorkspaceMemberDto
        {
            UserId = targetMember.UserId,
            WorkspaceId = targetMember.WorkspaceId,
            Role = targetMember.Role,
            JoinedAtUtc = targetMember.JoinedAtUtc,
            InvitedByUserId = targetMember.InvitedByUserId
        };

        return Result<WorkspaceMemberDto>.Success(memberDto);
    }

    public async Task<Result> RemoveMemberAsync(
        string workspaceId,
        string userId,
        string targetUserId,
        CancellationToken cancellationToken = default)
    {
        var currentMemberRole = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (currentMemberRole is null || (currentMemberRole != "owner" && currentMemberRole != "admin"))
        {
            return Result.Failure("You do not have permission to remove members");
        }

        var targetMember = await workspaceMemberRepository.GetMemberAsync(workspaceId, targetUserId, cancellationToken);
        if (targetMember is null)
        {
            return Result.Failure("Member not found");
        }

        if (targetMember.Role == WorkspaceRole.Owner)
        {
            return Result.Failure("Cannot remove the workspace owner");
        }

        if (currentMemberRole == "admin" && targetMember.Role == WorkspaceRole.Admin)
        {
            return Result.Failure("Admins cannot remove other admins");
        }

        workspaceMemberRepository.Remove(targetMember);
        await workspaceMemberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<WorkspaceInviteDto>> InviteUserAsync(
        string workspaceId,
        string userId,
        CreateWorkspaceInviteDto dto,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result<WorkspaceInviteDto>.Failure("Workspace not found");
        }

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (role is null || (role != "owner" && role != "admin"))
        {
            return Result<WorkspaceInviteDto>.Failure("You do not have permission to invite members");
        }

        var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
        {
            var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, existingUser.Id, cancellationToken);
            if (isMember)
            {
                return Result<WorkspaceInviteDto>.Failure("User is already a member of this workspace");
            }
        }

        var existingInvite = await workspaceInviteRepository.GetPendingInviteAsync(workspaceId, dto.Email, cancellationToken);
        if (existingInvite is not null)
        {
            return Result<WorkspaceInviteDto>.Failure("An active invite already exists for this email");
        }

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
        await workspaceInviteRepository.SaveChangesAsync(cancellationToken);

        var inviteDto = new WorkspaceInviteDto
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
        };

        return Result<WorkspaceInviteDto>.Success(inviteDto);
    }

    public async Task<Result<WorkspaceMemberDto>> AcceptInviteAsync(
        string token,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var invite = await workspaceInviteRepository.GetByTokenAsync(token, cancellationToken);
        if (invite is null)
        {
            return Result<WorkspaceMemberDto>.Failure("Invite not found");
        }

        if (invite.AcceptedAtUtc is not null)
        {
            return Result<WorkspaceMemberDto>.Failure("Invite has already been accepted");
        }

        if (invite.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Result<WorkspaceMemberDto>.Failure("Invite has expired");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<WorkspaceMemberDto>.Failure("User not found");
        }

        if (user.Email != invite.Email)
        {
            return Result<WorkspaceMemberDto>.Failure("This invite is for a different email address");
        }

        var isMember = await workspaceMemberRepository.IsMemberAsync(invite.WorkspaceId, userId, cancellationToken);
        if (isMember)
        {
            return Result<WorkspaceMemberDto>.Failure("You are already a member of this workspace");
        }

        var member = new WorkspaceMember
        {
            WorkspaceId = invite.WorkspaceId,
            UserId = userId,
            Role = invite.Role,
            JoinedAtUtc = DateTime.UtcNow,
            InvitedByUserId = invite.InvitedByUserId
        };

        await workspaceMemberRepository.AddAsync(member, cancellationToken);

        invite.AcceptedAtUtc = DateTime.UtcNow;
        workspaceInviteRepository.Update(invite);
        
        await workspaceInviteRepository.SaveChangesAsync(cancellationToken);

        var memberDto = new WorkspaceMemberDto
        {
            UserId = member.UserId,
            WorkspaceId = member.WorkspaceId,
            Role = member.Role,
            JoinedAtUtc = member.JoinedAtUtc,
            InvitedByUserId = member.InvitedByUserId
        };

        return Result<WorkspaceMemberDto>.Success(memberDto);
    }
}