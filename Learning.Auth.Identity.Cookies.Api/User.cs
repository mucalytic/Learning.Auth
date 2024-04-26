namespace Learning.Auth.Identity.Cookies.Api;

public record User
{
    public Username                    Username     { get; init; } = Username.Empty;
    public string                      PasswordHash { get; init; } = string.Empty;

    public IDictionary<string, string> Claims       { get; }       = new Dictionary<string, string>();
}
