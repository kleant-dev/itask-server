using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.CalendarEventDTOs;
using slender_server.Application.Interfaces.Services;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class CalendarEventsController(
    ICalendarEventService calendarEventService,
    IUserContext userContext,
    IValidator<CreateCalendarEventDto> createValidator,
    IValidator<UpdateCalendarEventDto> updateValidator)
    : ControllerBase
{
    [HttpPost("workspaces/{workspaceId}/calendar-events")]
    public async Task<ActionResult<CalendarEventDto>> Create(string workspaceId, [FromBody] CreateCalendarEventDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.CreateAsync(userId, dto with { WorkspaceId = workspaceId, CreatedById = userId }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { eventId = result.Value!.Id }, result.Value);
    }

    [HttpGet("workspaces/{workspaceId}/calendar-events")]
    public async Task<IActionResult> GetByWorkspace(string workspaceId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.GetByWorkspaceAsync(workspaceId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("workspaces/{workspaceId}/calendar-events/range")]
    public async Task<IActionResult> GetByRange(string workspaceId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.GetByDateRangeAsync(workspaceId, userId, fromUtc, toUtc, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("calendar-events/{eventId}")]
    public async Task<ActionResult<CalendarEventDto>> GetById(string eventId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.GetByIdAsync(eventId, userId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("calendar-events/{eventId}")]
    public async Task<ActionResult<CalendarEventDto>> Update(string eventId, [FromBody] UpdateCalendarEventDto dto, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.UpdateAsync(eventId, userId, dto, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("calendar-events/{eventId}")]
    public async Task<IActionResult> Delete(string eventId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await calendarEventService.DeleteAsync(eventId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return NoContent();
    }
}
