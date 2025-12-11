namespace Learning.Auth.Jwt.Revocation.Interfaces;

// methods are async because usually session is stored in redis or a database
public interface ITokenBlacklist
{
    Task BlacklistAsync(string token, DateTime expires);
    Task<bool> IsBlacklistedAsync(string token);
}
