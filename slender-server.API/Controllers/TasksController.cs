using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/")]
[ApiVersion(1.0)]
[Authorize]
public sealed class TasksController(
    ITaskService taskService,
    IUserContext userContext,
    IValidator<CreateTaskDto> createValidator,
    IValidator<UpdateTaskDto> updateValidator)
    : ControllerBase
{
    // GET api/v1/workspaces/{workspaceId}/tasks
    [HttpGet("workspaces/{workspaceId}/tasks")]
    public async Task<IActionResult> GetWorkspaceTasks(
        string workspaceId,
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? sort,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await taskService.GetWorkspaceTasksAsync(
            workspaceId,
            pagination,
            sort,
            status,
            cancellationToken);

        return Ok(result);
    }

    // GET api/v1/tasks/{taskId}
    [HttpGet("tasks/{taskId}", Name = nameof(GetTask))]
    public async Task<ActionResult<TaskDto>> GetTask(
        string taskId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await taskService.GetByIdAsync(taskId, userId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    // POST api/v1/tasks
    [HttpPost("tasks")]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await taskService.CreateTaskAsync(userId, dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTask), new { taskId = result.Value!.Id }, result.Value);
    }

    // PATCH api/v1/tasks/{taskId}
    [HttpPatch("tasks/{taskId}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        string taskId,
        [FromBody] UpdateTaskDto dto,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await taskService.UpdateTaskAsync(taskId, userId, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // DELETE api/v1/tasks/{taskId}
    [HttpDelete("tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(
        string taskId,
        CancellationToken cancellationToken)
    {
        var userId = await userContext.GetRequiredUserIdAsync(cancellationToken);
        var result = await taskService.DeleteTaskAsync(taskId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}

