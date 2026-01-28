namespace Learning.Auth.OAuth.Refresh.Tokens.Entities;

public class TokenInfo
{
    public string   AccessToken  { get; init; }
    public string   RefreshToken { get; init; }
    public DateTime Expiry       { get; init; }
}
