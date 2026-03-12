using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.ChannelDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class ChannelsController(
    IChannelService channelService,
    IUserContext userContext,
    IValidator<CreateChannelDto> createValidator,
    IValidator<UpdateChannelDto> updateValidator)
    : ControllerBase
{
    [HttpPost("workspaces/{workspaceId}/channels")]
    public async Task<ActionResult<ChannelDto>> Create(string workspaceId, [FromBody] CreateChannelDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.CreateAsync(userId, dto with { WorkspaceId = workspaceId }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { channelId = result.Value!.Id }, result.Value);
    }

    [HttpGet("workspaces/{workspaceId}/channels/dm")]
    public async Task<ActionResult<ChannelDto>> GetOrCreateDm(string workspaceId, [FromQuery] string otherUserId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.GetOrCreateDmAsync(userId, workspaceId, otherUserId, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("workspaces/{workspaceId}/channels")]
    public async Task<IActionResult> GetByWorkspace(string workspaceId, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.GetByWorkspaceAsync(workspaceId, userId, pagination, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("channels/{channelId}")]
    public async Task<ActionResult<ChannelDto>> GetById(string channelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.GetByIdAsync(channelId, userId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("channels/{channelId}")]
    public async Task<ActionResult<ChannelDto>> Update(string channelId, [FromBody] UpdateChannelDto dto, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.UpdateAsync(channelId, userId, dto, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("channels/{channelId}")]
    public async Task<IActionResult> Delete(string channelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.DeleteAsync(channelId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("channels/{channelId}/members")]
    public async Task<IActionResult> GetMembers(string channelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.GetMembersAsync(channelId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("channels/{channelId}/join")]
    public async Task<IActionResult> Join(string channelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.JoinAsync(channelId, userId, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("channels/{channelId}/leave")]
    public async Task<IActionResult> Leave(string channelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await channelService.LeaveAsync(channelId, userId, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
