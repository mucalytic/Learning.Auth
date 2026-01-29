using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using Learning.Auth.OAuth.Refresh.Tokens.Services;

namespace Learning.Auth.OAuth.Refresh.Tokens.Background;

public class TokenRefresher(IServiceProvider serviceProvider, ILogger<TokenRefresher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ITokenDatabase>();
                var context = scope.ServiceProvider.GetRequiredService<RefreshTokenContext>();
                var tokens = await database.GetAllExpiringTokensAsync(stoppingToken);
                foreach (var (patreonId, token) in tokens)
                {
                    using var response = await context.RefreshTokenAsync(token.RefreshToken, stoppingToken);
                    if (response.AccessToken is null)
                    {
                        logger.LogError("Failed to refresh token for {patreonId}", patreonId);
                        continue;
                    }
                    if (!int.TryParse(response.ExpiresIn, out var expiry) || expiry <= 0) expiry = 3600;
                    await database.SaveTokenAsync(patreonId, new TokenInfo
                    {
                        RefreshToken = response.RefreshToken ?? token.RefreshToken,
                        Expiry = DateTime.UtcNow.AddSeconds(expiry),
                        AccessToken = response.AccessToken,
                    }, stoppingToken);
                    logger.LogInformation("Refreshed and saved token for {patreonId}", patreonId);
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while refreshing tokens");
            }
        }
    }
}
