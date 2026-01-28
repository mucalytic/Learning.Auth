namespace Learning.Auth.OAuth.Refresh.Tokens.Entities;

public class TokenInfo(string? accessToken, string? refreshToken, TimeSpan? expiry)
{
    public string   AccessToken  { get; } = accessToken ?? string.Empty;
    public string   RefreshToken { get; } = refreshToken ?? string.Empty;
    public DateTime Expiry       { get; } = expiry.HasValue
                                                ? DateTime.UtcNow.AddSeconds(expiry.Value.TotalSeconds)
                                                : DateTime.UtcNow;
}
