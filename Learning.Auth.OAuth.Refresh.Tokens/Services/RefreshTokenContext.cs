using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Learning.Auth.OAuth.Refresh.Tokens.Services;

public class RefreshTokenContext(
    IOptionsMonitor<OAuthOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<RefreshTokenContext> logger)
{
    public async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", options.CurrentValue.ClientId },
            { "client_secret", options.CurrentValue.ClientSecret }
        };
        var requestContent = new FormUrlEncodedContent(tokenRequestParameters);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, options.CurrentValue.TokenEndpoint);
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        requestMessage.Content = requestContent;
        using var httpClient = httpClientFactory.CreateClient("patreon-refresh");
        requestMessage.Version = httpClient.DefaultRequestVersion;
        var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to refresh token: {StatusCode} {ReasonPhrase}",
                response.StatusCode, response.ReasonPhrase);
        }
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return OAuthTokenResponse.Success(JsonDocument.Parse(body));
    }
}
