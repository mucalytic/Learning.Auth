using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;

namespace Learning.Auth.OAuth.Refresh.Tokens.Services;

public class TokenDatabase : ITokenDatabase
{
    private readonly Dictionary<string, TokenInfo> _tokens = new();
    
    public Task<TokenInfo?> TryLoadAsync(string patreonId) =>
        _tokens.TryGetValue(patreonId, out var tokenInfo)
            ? Task.FromResult<TokenInfo?>(tokenInfo)
            : Task.FromResult<TokenInfo?>(null);

    public Task<bool> TrySaveAsync(string patreonId, TokenInfo tokenInfo)
    {
        _tokens[patreonId] = tokenInfo;
        return Task.FromResult(true);
    }
}
