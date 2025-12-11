using Learning.Auth.Jwt.Revocation.Interfaces;
using System.Collections.Concurrent;

namespace Learning.Auth.Jwt.Revocation.Blacklists;

public class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

    public Task BlacklistAsync(string session, DateTime expires)
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
