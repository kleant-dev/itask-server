using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.ProjectDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize] 
public sealed class ProjectsController(
    IProjectService projectService,
    IUserContext userContext,
    IValidator<CreateProjectDto> createValidator,
    IValidator<UpdateProjectDto> updateValidator)
    : ControllerBase
{
    // POST api/v1/workspaces/{workspaceId}/projects
    [HttpPost("workspaces/{workspaceId}/projects")]
    public async Task<ActionResult<ProjectDto>> CreateProject(
        string workspaceId,
        [FromBody] CreateProjectDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await projectService.CreateProjectAsync(workspaceId, userId, dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage});

        return CreatedAtAction(nameof(GetProject), new { projectId = result.Value!.Id }, result.Value);
    }

    // GET api/v1/workspaces/{workspaceId}/projects
    [HttpGet("workspaces/{workspaceId}/projects")]
    public async Task<IActionResult> GetWorkspaceProjects(
        string workspaceId,
        [FromQuery] PaginationParams pagination,
        [FromQuery] SortParams sort,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await projectService.GetWorkspaceProjectsAsync(workspaceId, userId, pagination, sort, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage});

        return Ok(result.Value);
    }

    // GET api/v1/projects/{projectId}
    [HttpGet("projects/{projectId}", Name = nameof(GetProject))]
    public async Task<ActionResult<ProjectDto>> GetProject(
        string projectId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await projectService.GetProjectByIdAsync(projectId, userId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorMessage});

        return Ok(result.Value);
    }

    // PATCH api/v1/projects/{projectId}
    [HttpPatch("projects/{projectId}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(
        string projectId,
        [FromBody] UpdateProjectDto dto,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await projectService.UpdateProjectAsync(projectId, userId, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorType.Equals(ErrorType.NotFound)
                ? NotFound(new { error = result.ErrorMessage})
                : BadRequest(new { error = result.ErrorMessage});
        }

        return Ok(result.Value);
    }

    // POST api/v1/projects/{projectId}/archive
    [HttpPost("projects/{projectId}/archive")]
    public async Task<IActionResult> ArchiveProject(
        string projectId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await projectService.ArchiveProjectAsync(projectId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorType.Equals(ErrorType.NotFound)
                ? NotFound(new { error = result.ErrorMessage})
                : BadRequest(new { error = result.ErrorMessage});
        }

        return NoContent();
    }
}

