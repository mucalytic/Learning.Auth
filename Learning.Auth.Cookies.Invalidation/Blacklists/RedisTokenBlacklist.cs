using Learning.Auth.Cookies.Invalidation.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Learning.Auth.Cookies.Invalidation.Blacklists;

public class RedisTokenBlacklist(IDistributedCache cache) : ITokenBlacklist
{
    public Task BlacklistAsync(string session, DateTimeOffset expires) =>
        cache.SetStringAsync(session, "revoked", new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expires
        });

    public async Task<bool> IsBlacklistedAsync(string session)
    {
        var value = await cache.GetStringAsync(session);
        return value == "revoked";
    }
}
