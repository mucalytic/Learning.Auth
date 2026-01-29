using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Learning.Auth.OAuth.Refresh.Tokens.EventHandlers;

public class OnCreatingTicketHandler(ITokenDatabase database, ILogger<OnCreatingTicketHandler> logger)
{
    public async Task HandleAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        var patreonId = json.GetProperty("data").GetProperty("id").GetString();
        if (patreonId is null) return;
        context.Identity?.AddClaim(new Claim("patreonId", patreonId));
        if (context.AccessToken is null || context.RefreshToken is null)
        {
            logger.LogError("Failed to get access token for {patreonId}", patreonId);
            return;
        }
        await database.SaveTokenAsync(patreonId, new TokenInfo
        {
            Expiry = DateTime.UtcNow.Add(context.ExpiresIn ?? TimeSpan.FromSeconds(3600)),
            RefreshToken = context.RefreshToken,
            AccessToken = context.AccessToken
        }, context.HttpContext.RequestAborted);
        logger.LogInformation("Saved token for {patreonId}", patreonId);
    }
}
