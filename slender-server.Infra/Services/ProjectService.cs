using Microsoft.Extensions.Logging;
using slender_server.Application.DTOs.ProjectDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class ProjectService(
    IProjectRepository projectRepository,
    IWorkspaceRepository workspaceRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    ISortingService sortingService,
    ILogger<ProjectService> logger)
    : IProjectService
{
    private static bool IsPrivileged(WorkspaceRole? role) =>
        role is WorkspaceRole.Owner or WorkspaceRole.Admin;

    public async Task<Result<ProjectDto>> CreateProjectAsync(
        string workspaceId,
        string userId,
        CreateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        // Ensure workspace exists and user is a member
        var workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return Result<ProjectDto>.Failure("Workspace not found");

        var role = await workspaceRepository.GetMemberRoleAsync(workspaceId, userId, cancellationToken);
        if (!IsPrivileged(role))
            return Result<ProjectDto>.Failure("You do not have permission to create projects in this workspace");

        // Ignore client-supplied workspace/owner IDs for safety
        var project = dto with { WorkspaceId = workspaceId, OwnerId = userId };
        var entity = project.ToEntity();

        await projectRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(entity.ToDto());
    }

    public async Task<Result<PagedResponse<ProjectDto>>> GetWorkspaceProjectsAsync(
        string workspaceId,
        string userId,
        PaginationParams pagination,
        SortParams sort,
        CancellationToken cancellationToken = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, cancellationToken);
        if (!isMember)
            return Result<PagedResponse<ProjectDto>>.Failure("You do not have access to this workspace");

        var pagedResult = await projectRepository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            p => p.WorkspaceId == workspaceId,
            null,
            cancellationToken);

        var pagedResponse = paginationService.MapToPagedResponse(pagedResult, p => p.ToDto());

        return Result<PagedResponse<ProjectDto>>.Success(pagedResponse);
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(
        string projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdWithDetailsAsync(projectId, cancellationToken);
        if (project is null)
            return Result<ProjectDto>.Failure("Project not found");

        var isWorkspaceMember = await workspaceMemberRepository.IsMemberAsync(project.WorkspaceId, userId, cancellationToken);
        if (!isWorkspaceMember)
            return Result<ProjectDto>.Failure("You do not have access to this project");

        return Result<ProjectDto>.Success(project.ToDto());
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(
        string projectId,
        string userId,
        UpdateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<ProjectDto>.Failure("Project not found");

        var role = await workspaceRepository.GetMemberRoleAsync(project.WorkspaceId, userId, cancellationToken);
        if (!IsPrivileged(role) && project.OwnerId != userId)
            return Result<ProjectDto>.Failure("You do not have permission to update this project");

        dto.ApplyTo(project);
        await projectRepository.UpdateAsync(project,cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(project.ToDto());
    }

    public async Task<Result> ArchiveProjectAsync(
        string projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("Project not found");

        var role = await workspaceRepository.GetMemberRoleAsync(project.WorkspaceId, userId, cancellationToken);
        if (!IsPrivileged(role) && project.OwnerId != userId)
            return Result.Failure("You do not have permission to archive this project");

        project.ArchivedAtUtc = DateTime.UtcNow;
        project.UpdatedAtUtc = DateTime.UtcNow;
        await projectRepository.UpdateAsync(project,cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

