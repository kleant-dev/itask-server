using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using slender_server.Application.Common.Settings;
using slender_server.Application.DTOs.Auth;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Entities;
using slender_server.Infra.Database;

namespace slender_server.Infra.Auth;

public sealed class AuthService(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext,
    ITokenProvider tokenProvider,
    IOptions<JwtAuthOptions> options)
    : IAuthService
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokenDto>> RegisterAsync(
        RegisterUserDto registerUserDto,
        CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
            await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

            // Create Identity User
            var identityUser = new IdentityUser
            {
                Email = registerUserDto.Email,
                UserName = registerUserDto.Email
            };

            var createUserResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = createUserResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result<AccessTokenDto>.Failure(errors);
            }

            // Add role
            var addToRoleResult = await userManager.AddToRoleAsync(identityUser, "Member");
            if (!addToRoleResult.Succeeded)
            {
                var errors = addToRoleResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result<AccessTokenDto>.Failure(errors);
            }

            // Create Application User
            var user = registerUserDto.ToEntity();
            user.IdentityId = identityUser.Id;
            applicationDbContext.Users.Add(user);
            await applicationDbContext.SaveChangesAsync(cancellationToken);

            // Generate tokens
            var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!, ["Member"]);
            var accessTokens = tokenProvider.Create(tokenRequest);

            // Save refresh token
            var refreshToken = CreateRefreshToken(identityUser.Id, accessTokens.RefreshToken);
            identityDbContext.RefreshTokens.Add(refreshToken);
            await identityDbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<AccessTokenDto>.Success(accessTokens);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<AccessTokenDto>.Failure($"Registration failed: {ex.Message}");
        }
    }

    public async Task<Result<AccessTokenDto>> LoginAsync(
        LoginUserDto loginUserDto,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (identityUser is null)
        {
            return Result<AccessTokenDto>.Failure("Invalid credentials");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password);
        if (!isPasswordValid)
        {
            return Result<AccessTokenDto>.Failure("Invalid credentials");
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!, roles);
        var accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = CreateRefreshToken(identityUser.Id, accessTokens.RefreshToken);
        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        return Result<AccessTokenDto>.Success(accessTokens);
    }

    public async Task<Result<AccessTokenDto>> RefreshTokenAsync(
        RefreshTokenDto refreshTokenDto,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await identityDbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            return Result<AccessTokenDto>.Failure("Invalid refresh token");
        }

        if (refreshToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Result<AccessTokenDto>.Failure("Refresh token expired");
        }

        var user = await identityDbContext.Users
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result<AccessTokenDto>.Failure("User not found");
        }

        var roles = await userManager.GetRolesAsync(user);
        var tokenRequest = new TokenRequest(user.Id, user.Email!, roles);
        var accessTokens = tokenProvider.Create(tokenRequest);

        // Update refresh token
        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        return Result<AccessTokenDto>.Success(accessTokens);
    }

    public async Task<Result> LogoutAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var token = await identityDbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId, cancellationToken);

        if (token is not null)
        {
            identityDbContext.RefreshTokens.Remove(token);
            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<AccessTokenDto>> GoogleLoginAsync(
        string googleToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            var email = payload.Email;
            var name = payload.Name;
            var emailVerified = payload.EmailVerified;

            var identityUser = await userManager.FindByEmailAsync(email);

            // New user registration
            if (identityUser is null)
            {
                await using var transaction = await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
                    await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

                    identityUser = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = emailVerified
                    };

                    var createResult = await userManager.CreateAsync(identityUser);
                    if (!createResult.Succeeded)
                    {
                        var errors = createResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                        return Result<AccessTokenDto>.Failure(errors);
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(identityUser, "Member");
                    if (!addToRoleResult.Succeeded)
                    {
                        var errors = addToRoleResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                        return Result<AccessTokenDto>.Failure(errors);
                    }

                    var applicationUser = new User
                    {
                        Id = User.NewId(),
                        IdentityId = identityUser.Id,
                        Email = email,
                        Name = name,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    applicationDbContext.Users.Add(applicationUser);
                    await applicationDbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else if (!identityUser.EmailConfirmed && emailVerified)
            {
                identityUser.EmailConfirmed = true;
                await userManager.UpdateAsync(identityUser);
            }

            var roles = await userManager.GetRolesAsync(identityUser);
            var tokenRequest = new TokenRequest(identityUser.Id, email, roles);
            var accessTokenDto = tokenProvider.Create(tokenRequest);

            var refreshToken = CreateRefreshToken(identityUser.Id, accessTokenDto.RefreshToken);
            identityDbContext.RefreshTokens.Add(refreshToken);
            await identityDbContext.SaveChangesAsync(cancellationToken);

            return Result<AccessTokenDto>.Success(accessTokenDto);
        }
        catch (InvalidJwtException)
        {
            return Result<AccessTokenDto>.Failure("Invalid Google token");
        }
        catch (Exception ex)
        {
            return Result<AccessTokenDto>.Failure($"Google login failed: {ex.Message}");
        }
    }

    private RefreshToken CreateRefreshToken(string userId, string token)
    {
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays)
        };
    }
}