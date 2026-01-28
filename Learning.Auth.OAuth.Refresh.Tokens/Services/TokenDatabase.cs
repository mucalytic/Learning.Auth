using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using System.Text.Json;

namespace Learning.Auth.OAuth.Refresh.Tokens.Services;

public class TokenDatabase(IWebHostEnvironment environment) : ITokenDatabase
{
    private readonly string _path = Path.Combine(environment.ContentRootPath, "tokens.json");

    public Task<TokenInfo?> TryLoadAsync(string patreonId)
    {
        if (File.Exists(_path))
        {
            var text = File.ReadAllText(_path);
            var tokens = JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(text)
                                             ?? new Dictionary<string, TokenInfo>();
            return tokens.TryGetValue(patreonId, out var tokenInfo)
                ? Task.FromResult<TokenInfo?>(tokenInfo)
                : Task.FromResult<TokenInfo?>(null);
        }
        File.Create(_path).Close();
        return Task.FromResult<TokenInfo?>(null);
    }

    public Task<bool> TrySaveAsync(string patreonId, TokenInfo tokenInfo)
    {
        var tokens = new Dictionary<string, TokenInfo>();
        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            tokens = JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(json)
                                         ?? new Dictionary<string, TokenInfo>();
            tokens[patreonId] = tokenInfo;
        }
        File.WriteAllText(_path, JsonSerializer.Serialize(tokens));
        return Task.FromResult(true);
    }
}
