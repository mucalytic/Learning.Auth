using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Services;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net;
using Polly;

namespace Learning.Auth.OAuth.Refresh.Tokens.HttpClients;

public class PatreonHttpClient(HttpClient client, ITokenDatabase database, IOptionsSnapshot<OAuthOptions> snapshot)
{
    public async Task<string> GetInfo(string patreonId)
    {
        var info = await database.TryGetTokenAsync(patreonId, CancellationToken.None);
        if (info is null) return string.Empty;
        var options = snapshot.Get("patreon");
        using var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", info.AccessToken);
        request.Headers.Add("PatreonId", patreonId);
        using var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }

    public static IAsyncPolicy<HttpResponseMessage> HandleUnauthorisedRequest(
        IServiceProvider services, HttpRequestMessage request) =>
        Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.Unauthorized)
              .RetryAsync(1, async (_, _) =>
              {
                  var patreonId = request.Headers.GetValues("PatreonId").FirstOrDefault();
                  if (patreonId is null) return;
                  using var scope = services.CreateScope();
                  var database = scope.ServiceProvider.GetRequiredService<ITokenDatabase>();
                  var info = await database.TryGetTokenAsync(patreonId, CancellationToken.None);
                  if (info is null) return;
                  var context = scope.ServiceProvider.GetRequiredService<RefreshTokenContext>();
                  using var response = await context.RefreshTokenAsync(info.RefreshToken, CancellationToken.None);
                  var logger = scope.ServiceProvider.GetRequiredService<ILogger<PatreonHttpClient>>();
                  if (response.AccessToken is null || response.RefreshToken is null)
                  {
                      logger.LogError("Failed to get access token for {patreonId}", patreonId);
                      return;
                  }
                  if (!int.TryParse(response.ExpiresIn, out var expiry) || expiry <= 0) expiry = 3600;
                  await database.SaveTokenAsync(patreonId, new TokenInfo
                  {
                      Expiry = DateTime.UtcNow.AddSeconds(expiry),
                      RefreshToken = response.RefreshToken,
                      AccessToken = response.AccessToken
                  }, CancellationToken.None);
                  logger.LogInformation("Refreshed and saved token for {patreonId}", patreonId);
                  request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", response.AccessToken);
              });
}
