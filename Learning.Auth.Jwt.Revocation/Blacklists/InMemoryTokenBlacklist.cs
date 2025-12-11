using Learning.Auth.Jwt.Revocation.Interfaces;
using System.Collections.Concurrent;

namespace Learning.Auth.Jwt.Revocation.Blacklists;

public class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

    public Task BlacklistAsync(string token, DateTime expires)
    {
        if (expires > DateTime.UtcNow)
        {
            _blacklist[token] = expires;
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string token)
    {
        if (_blacklist.TryGetValue(token, out var expires))
        {
            if (DateTime.UtcNow >= expires)
            {
                _blacklist.TryRemove(token, out _);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
