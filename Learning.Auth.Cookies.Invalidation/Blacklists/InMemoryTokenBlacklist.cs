using Learning.Auth.Cookies.Invalidation.Interfaces;
using System.Collections.Concurrent;

namespace Learning.Auth.Cookies.Invalidation.Blacklists;

public class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _blacklist = new();

    public Task BlacklistAsync(string session, DateTimeOffset expires)
    {
        if (expires > DateTime.UtcNow)
        {
            _blacklist[session] = expires;
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string session)
    {
        if (_blacklist.TryGetValue(session, out var expires))
        {
            if (DateTime.UtcNow >= expires)
            {
                _blacklist.TryRemove(session, out _);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
