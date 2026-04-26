using System.Security.Claims;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.UserDTOs;
using slender_server.Application.Interfaces.Services;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion(1.0)]
[Authorize]
public sealed class UsersController(
    IUserService userService,
    IValidator<UpdateUserDto> updateValidator)
    : ControllerBase
{
    private string RequiredIdentityId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found in claims");

    // GET api/v1/users/me
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var result = await userService.GetCurrentUserAsync(RequiredIdentityId, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.ErrorMessage });
    }

    // PATCH api/v1/users/me
    [HttpPatch("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var result = await userService.UpdateCurrentUserAsync(RequiredIdentityId, dto, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.ErrorMessage });
    }

    // DELETE api/v1/users/me
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
    {
        var result = await userService.DeleteAccountAsync(RequiredIdentityId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.ErrorMessage});
    }
}