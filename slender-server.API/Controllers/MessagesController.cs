using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class MessagesController(
    IMessageService messageService,
    IUserContext userContext,
    IValidator<CreateMessageDto> createValidator,
    IValidator<UpdateMessageDto> updateValidator)
    : ControllerBase
{
    [HttpPost("channels/{channelId}/messages")]
    public async Task<ActionResult<MessageDto>> Create(string channelId, [FromBody] CreateMessageDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.CreateAsync(userId, dto with { ChannelId = channelId }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { messageId = result.Value!.Id }, result.Value);
    }

    [HttpGet("channels/{channelId}/messages")]
    public async Task<IActionResult> GetByChannel(string channelId, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.GetByChannelAsync(channelId, userId, pagination, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("messages/{messageId}", Name = nameof(GetById))]
    public async Task<ActionResult<MessageDto>> GetById(string messageId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.GetByIdAsync(messageId, userId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("messages/{messageId}")]
    public async Task<ActionResult<MessageDto>> Update(string messageId, [FromBody] UpdateMessageDto dto, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.UpdateAsync(messageId, userId, dto, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> Delete(string messageId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await messageService.DeleteAsync(messageId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return NoContent();
    }
}
