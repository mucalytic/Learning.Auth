using Learning.Auth.OAuth.Refresh.Tokens.Entities;

namespace Learning.Auth.OAuth.Refresh.Tokens.Interfaces;

public interface ITokenDatabase
{
    Task<IEnumerable<(string, TokenInfo)>> GetAllExpiringTokensAsync(CancellationToken cancellationToken);
    Task<TokenInfo?> TryGetTokenAsync(string patreonId, CancellationToken cancellationToken);
    Task SaveTokenAsync(string patreonId, TokenInfo tokenInfo, CancellationToken cancellationToken);
}
