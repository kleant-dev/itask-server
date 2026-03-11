using slender_server.Application.DTOs.ProjectDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.Interfaces.Services;

public interface IProjectService
{
    Task<Result<ProjectDto>> CreateProjectAsync(
        string workspaceId,
        string userId,
        CreateProjectDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<ProjectDto>>> GetWorkspaceProjectsAsync(
        string workspaceId,
        string userId,
        PaginationParams pagination,
        SortParams sort,
        CancellationToken cancellationToken = default);

    Task<Result<ProjectDto>> GetProjectByIdAsync(
        string projectId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<Result<ProjectDto>> UpdateProjectAsync(
        string projectId,
        string userId,
        UpdateProjectDto dto,
        CancellationToken cancellationToken = default);

    Task<Result> ArchiveProjectAsync(
        string projectId,
        string userId,
        CancellationToken cancellationToken = default);
}

