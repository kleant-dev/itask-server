using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;

namespace slender_server.API.Controllers;

/// <summary>
/// REST endpoint for fetching message history.
/// Real-time sending/editing/deleting is handled via SignalR (ChatHub).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class MessagesController(
    IMessageService messageService,
    IUserContext userContext)
    : ControllerBase
{
    /// <summary>
    /// GET api/v1/channels/{channelId}/messages
    /// Returns paginated message history for a channel (oldest-first).
    /// </summary>
    [HttpGet("channels/{channelId}/messages")]
    public async Task<IActionResult> GetByChannel(
        string channelId,
        [FromQuery] PaginationParams pagination,
        CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.GetByChannelAsync(channelId, userId, pagination, ct);

        if (!result.IsSuccess)
            return result.ErrorType.Equals(ErrorType.Forbidden)
                ? Forbid()
                : BadRequest(new { error = result.ErrorMessage});

        return Ok(result.Value);
    }
}