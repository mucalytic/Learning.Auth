namespace Learning.Auth.Cookies.Invalidation.Interfaces;

// methods are async because usually session is stored in redis or a database
public interface ITokenBlacklist
{
    Task BlacklistAsync(string session, DateTime expires);
    Task<bool> IsBlacklistedAsync(string session);
}
