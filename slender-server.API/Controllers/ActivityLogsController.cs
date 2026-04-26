using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.ActivityLog;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/activity-logs")]
[ApiVersion(1.0)]
[Authorize]
public sealed class ActivityLogsController(
    IActivityLogService activityLogService,
    IUserContext userContext,
    IValidator<CreateActivityLogDto> createValidator)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ActivityLogDto>> Create(string workspaceId, [FromBody] CreateActivityLogDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var result = await activityLogService.CreateAsync(dto with { WorkspaceId = workspaceId, ActorId = await userContext.GetRequiredUserIdAsync(ct) }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetByWorkspace), new { workspaceId }, result.Value);
    }

    [HttpGet(Name = nameof(GetByWorkspace))]
    public async Task<IActionResult> GetByWorkspace(string workspaceId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await activityLogService.GetWorkspaceLogsAsync(workspaceId, userId, ct);
        if (!result.IsSuccess)
            return result.ErrorType.Equals(ErrorType.Forbidden) ? Forbid() : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Value);
    }
}
