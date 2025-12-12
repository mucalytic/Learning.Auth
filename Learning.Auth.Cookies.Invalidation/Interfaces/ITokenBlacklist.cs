namespace Learning.Auth.Cookies.Invalidation.Interfaces;

public interface ITokenBlacklist
{
    Task BlacklistAsync(string session, DateTimeOffset expires);
    Task<bool> IsBlacklistedAsync(string session);
}
