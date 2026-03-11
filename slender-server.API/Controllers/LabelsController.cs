using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.LabelDTOs;
using slender_server.Application.Interfaces.Services;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class LabelsController(
    ILabelService labelService,
    IUserContext userContext,
    IValidator<CreateLabelDto> createValidator,
    IValidator<UpdateLabelDto> updateValidator)
    : ControllerBase
{
    [HttpPost("workspaces/{workspaceId}/labels")]
    public async Task<ActionResult<LabelDto>> Create(string workspaceId, [FromBody] CreateLabelDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await labelService.CreateLabelAsync(userId, dto with { WorkspaceId = workspaceId }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { labelId = result.Value!.Id }, result.Value);
    }

    [HttpGet("workspaces/{workspaceId}/labels")]
    public async Task<IActionResult> GetByWorkspace(string workspaceId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await labelService.GetWorkspaceLabelsAsync(workspaceId, userId, ct);
        if (!result.IsSuccess) return result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase) ? Forbid() : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("labels/{labelId}", Name = nameof(GetById))]
    public async Task<ActionResult<LabelDto>> GetById(string labelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await labelService.GetByIdAsync(labelId, userId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("labels/{labelId}")]
    public async Task<ActionResult<LabelDto>> Update(string labelId, [FromBody] UpdateLabelDto dto, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await labelService.UpdateLabelAsync(labelId, userId, dto, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("labels/{labelId}")]
    public async Task<IActionResult> Delete(string labelId, CancellationToken ct)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);
        var result = await labelService.DeleteLabelAsync(labelId, userId, ct);
        if (!result.IsSuccess)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return NoContent();
    }
}
