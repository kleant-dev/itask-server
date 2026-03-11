using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.DTOs.WorkspaceInviteDTOs;
using slender_server.Application.DTOs.WorkspaceMemberDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/workspaces")]
[ApiVersion(1.0)]
[Authorize]
public sealed class WorkspacesController(
    IWorkspaceService workspaceService,
    IUserContext userContext,
    IValidator<CreateWorkspaceDto> createValidator,
    IValidator<UpdateWorkspaceDto> updateValidator)
    : ControllerBase
{
    // ─── Workspaces ──────────────────────────────────────────────────────────────

    // POST api/v1/workspaces
    [HttpPost]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace(
        [FromBody] CreateWorkspaceDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.CreateWorkspaceAsync(userId, dto, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetWorkspace), new { workspaceId = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    // GET api/v1/workspaces
    [HttpGet]
    public async Task<IActionResult> GetWorkspaces(
        [FromQuery] PaginationParams pagination,
        [FromQuery] SortParams sort,
        [FromQuery] string? fields,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.GetUserWorkspacesAsync(userId, pagination, sort, fields, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    // GET api/v1/workspaces/{workspaceId}
    [HttpGet("{workspaceId}", Name = nameof(GetWorkspace))]
    public async Task<ActionResult<WorkspaceDto>> GetWorkspace(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.GetWorkspaceByIdAsync(workspaceId, userId, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    // PATCH api/v1/workspaces/{workspaceId}
    [HttpPatch("{workspaceId}")]
    public async Task<ActionResult<WorkspaceDto>> UpdateWorkspace(
        string workspaceId,
        [FromBody] UpdateWorkspaceDto dto,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.UpdateWorkspaceAsync(workspaceId, userId, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // DELETE api/v1/workspaces/{workspaceId}
    [HttpDelete("{workspaceId}")]
    public async Task<IActionResult> DeleteWorkspace(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.DeleteWorkspaceAsync(workspaceId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    // ─── Members ─────────────────────────────────────────────────────────────────

    // GET api/v1/workspaces/{workspaceId}/members
    [HttpGet("{workspaceId}/members")]
    public async Task<IActionResult> GetMembers(
        string workspaceId,
        [FromQuery] PaginationParams pagination,
        [FromQuery] SortParams sort,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.GetWorkspaceMembersAsync(
            workspaceId, userId, pagination, sort, role, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase)
                ? Forbid()
                : NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // PATCH api/v1/workspaces/{workspaceId}/members/{targetUserId}
    [HttpPatch("{workspaceId}/members/{targetUserId}")]
    public async Task<ActionResult<WorkspaceMemberDto>> UpdateMemberRole(
        string workspaceId,
        string targetUserId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.UpdateMemberRoleAsync(
            workspaceId, userId, targetUserId, request.Role, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // DELETE api/v1/workspaces/{workspaceId}/members/{targetUserId}
    [HttpDelete("{workspaceId}/members/{targetUserId}")]
    public async Task<IActionResult> RemoveMember(
        string workspaceId,
        string targetUserId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.RemoveMemberAsync(
            workspaceId, userId, targetUserId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    // ─── Invites ─────────────────────────────────────────────────────────────────

    // POST api/v1/workspaces/{workspaceId}/invites
    [HttpPost("{workspaceId}/invites")]
    public async Task<ActionResult<WorkspaceInviteDto>> InviteUser(
        string workspaceId,
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);

        // Build the DTO the service expects — Email and Role come from the request body.
        // WorkspaceId and InvitedByUserId are server-derived (route + auth), not client-provided.
        var dto = new CreateWorkspaceInviteDto
        {
            WorkspaceId = workspaceId,
            Email = request.Email,
            Role = request.Role,
            InvitedByUserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        var result = await workspaceService.InviteUserAsync(workspaceId, userId, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // POST api/v1/workspaces/invites/accept
    [HttpPost("invites/accept")]
    public async Task<ActionResult<WorkspaceMemberDto>> AcceptInvite(
        [FromBody] AcceptInviteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await workspaceService.AcceptInviteAsync(request.Token, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // ─── Request Body Records ────────────────────────────────────────────────────

    public sealed record UpdateMemberRoleRequest(string Role);
    public sealed record AcceptInviteRequest(string Token);

    /// <summary>
    /// Slim invite request — only client-provided fields.
    /// WorkspaceId comes from the route, InvitedByUserId from the JWT, Token/Expiry set server-side.
    /// </summary>
    public sealed record InviteUserRequest(
        string Email,
        Domain.Entities.WorkspaceRole Role);
}