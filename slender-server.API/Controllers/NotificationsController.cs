using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/notifications")]
[ApiVersion(1.0)]
[Authorize]
public sealed class NotificationsController(INotificationService notificationService, IUserContext userContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await notificationService.GetUserNotificationsAsync(userId, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage});
        return Ok(result.Value);
    }

    [HttpPatch("{notificationId}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(string notificationId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await notificationService.UpdateNotificationAsync(notificationId, userId, new UpdateNotificationDto { ReadAtUtc = DateTime.UtcNow }, ct);
        if (!result.IsSuccess)
            return result.ErrorType.Equals(ErrorType.NotFound) ? NotFound(new { error = result.ErrorMessage}) : BadRequest(new { error = result.ErrorMessage});
        return Ok(result.Value);
    }
}
