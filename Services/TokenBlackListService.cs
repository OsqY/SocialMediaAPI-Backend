using Microsoft.Extensions.Caching.Distributed;

namespace SocialMediaAPI.Services;

public class TokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public TokenBlacklistService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task BlackListTokenAsync(string token, TimeSpan expirationTime)
    {
        await _cache.SetStringAsync(
            token,
            "blacklisted",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime }
        );
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return await _cache.GetStringAsync(token) != null;
    }
}
