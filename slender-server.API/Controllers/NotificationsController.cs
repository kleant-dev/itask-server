using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Interfaces.Services;

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
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("{notificationId}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(string notificationId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await notificationService.UpdateNotificationAsync(notificationId, userId, new UpdateNotificationDto { ReadAtUtc = DateTime.UtcNow }, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
