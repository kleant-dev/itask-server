using System.Security.Claims;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using slender_server.Application.DTOs.Auth;
using slender_server.Application.Interfaces.Services;

namespace slender_server.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersionNeutral]
public sealed class AuthController(
    IAuthService authService,
    IValidator<RegisterUserDto> registerValidator,
    IValidator<LoginUserDto> loginValidator,
    IValidator<RefreshTokenDto> refreshValidator)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokenDto>> Register(
        RegisterUserDto registerUserDto,
        CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(registerUserDto, cancellationToken);

        var result = await authService.RegisterAsync(registerUserDto, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Errors is not null)
            {
                return Problem(
                    detail: "Unable to register user",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errors", result.Errors } });
            }

            return Problem(
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(result.Value);
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenDto>> Login(
        LoginUserDto loginUserDto,
        CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(loginUserDto, cancellationToken);

        var result = await authService.LoginAsync(loginUserDto, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }
    
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokenDto>> Refresh(
        RefreshTokenDto refreshTokenDto,
        CancellationToken cancellationToken)
    {
        await refreshValidator.ValidateAndThrowAsync(refreshTokenDto, cancellationToken);

        var result = await authService.RefreshTokenAsync(refreshTokenDto, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenDto refreshTokenDto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User identifier not found in claims");

        var result = await authService.LogoutAsync(userId, refreshTokenDto.RefreshToken, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [AllowAnonymous]
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.GoogleLoginAsync(request.GoogleToken, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }

    public record GoogleLoginRequest(string GoogleToken);
}
