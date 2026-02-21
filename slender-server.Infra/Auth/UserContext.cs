
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using slender_server.Application.Interfaces.Services;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Auth;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    IMemoryCache memoryCache) : IUserContext
{
    private const string CacheKeyPrefix = "users:id:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        string? identityId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (identityId is null)
        {
            return null;
        }

        string cacheKey = $"{CacheKeyPrefix}{identityId}";

        string? userId = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);

            return await userRepository.GetIdByIdentityIdAsync(identityId, cancellationToken);
        });

        return userId;
    }

    public async Task<string> GetRequiredUserIdAsync(CancellationToken cancellationToken = default)
    {
        string? userId = await GetUserIdAsync(cancellationToken);
        
        if (userId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return userId;
    }

    public void InvalidateCache()
    {
        string? identityId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (identityId is not null)
        {
            string cacheKey = $"{CacheKeyPrefix}{identityId}";
            memoryCache.Remove(cacheKey);
        }
    }
}