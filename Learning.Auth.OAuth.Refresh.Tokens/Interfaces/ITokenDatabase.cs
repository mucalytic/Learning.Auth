using Learning.Auth.OAuth.Refresh.Tokens.Entities;

namespace Learning.Auth.OAuth.Refresh.Tokens.Interfaces;

public interface ITokenDatabase
{
    Task<TokenInfo?> TryLoadAsync(string patreonId);
    Task<bool>       TrySaveAsync(string patreonId, TokenInfo tokenInfo);
}
