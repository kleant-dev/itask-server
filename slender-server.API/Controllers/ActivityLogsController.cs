using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.ActivityLog;
using slender_server.Application.Interfaces.Services;

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
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetByWorkspace), new { workspaceId }, result.Value);
    }

    [HttpGet(Name = nameof(GetByWorkspace))]
    public async Task<IActionResult> GetByWorkspace(string workspaceId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await activityLogService.GetWorkspaceLogsAsync(workspaceId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
