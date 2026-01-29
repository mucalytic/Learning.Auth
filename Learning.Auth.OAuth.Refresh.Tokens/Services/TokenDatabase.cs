using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using System.Text.Json;

namespace Learning.Auth.OAuth.Refresh.Tokens.Services;

public class TokenDatabase(IWebHostEnvironment environment) : ITokenDatabase
{
    private readonly string _path = Path.Combine(environment.ContentRootPath, "tokens.json");

    public async Task<IEnumerable<(string, TokenInfo)>> GetAllExpiringTokensAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path)) return [];
        var text = await File.ReadAllTextAsync(_path, cancellationToken);
        var tokens = JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(text)
                                         ?? new Dictionary<string, TokenInfo>();
        var expiring = tokens.Where(kvp => kvp.Value.Expiry.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(5));
        return expiring.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }

    public async Task<TokenInfo?> TryGetTokenAsync(string patreonId, CancellationToken cancellationToken)
    {
        if (!File.Exists(_path)) return null;
        var text = await File.ReadAllTextAsync(_path, cancellationToken);
        var tokens = JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(text)
                                         ?? new Dictionary<string, TokenInfo>();
        return tokens.GetValueOrDefault(patreonId);
    }

    public async Task SaveTokenAsync(string patreonId, TokenInfo tokenInfo, CancellationToken cancellationToken)
    {
        var tokens = new Dictionary<string, TokenInfo>();
        if (File.Exists(_path))
        {
            var storedJson = await File.ReadAllTextAsync(_path, cancellationToken);
            tokens = JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(storedJson)
                                         ?? new Dictionary<string, TokenInfo>();
        }
        tokens[patreonId] = tokenInfo;
        var updatedJson = JsonSerializer.Serialize(tokens);
        await File.WriteAllTextAsync(_path, updatedJson, cancellationToken);
    }
}
