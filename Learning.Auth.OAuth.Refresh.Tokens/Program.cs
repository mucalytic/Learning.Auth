using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Background;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using Learning.Auth.OAuth.Refresh.Tokens.Services;
using Microsoft.AspNetCore.Authentication;
using Learning.Auth.OAuth.Refresh.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OAuthConfig>("patreon", builder.Configuration.GetSection("patreon"));
builder.Services.AddAuthentication("cookie").AddCookie("cookie").AddOAuth("patreon", options =>
{
    options.ClientId = builder.Configuration["patreon:clientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["patreon:clientSecret"] ?? string.Empty;
    options.UserInformationEndpoint = "https://www.patreon.com/api/oauth2/v2/identity";
    options.AuthorizationEndpoint = "https://www.patreon.com/oauth2/authorize";
    options.TokenEndpoint = "https://www.patreon.com/api/oauth2/token";
    options.CallbackPath = "/oauth/callback";
    options.SignInScheme = "cookie";
    options.Scope.Clear();
    options.Scope.Add("identity");
    options.Events.OnCreatingTicket = async context =>
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
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Failed to get access token for {patreonId}", patreonId);
            return;
        }
        var database = context.HttpContext.RequestServices.GetRequiredService<ITokenDatabase>();
        var success = await database.TrySaveTokenAsync(patreonId, new TokenInfo
        {
            Expiry = DateTime.UtcNow.Add(context.ExpiresIn ?? TimeSpan.FromSeconds(3600)),
            AccessToken = context.AccessToken,
            RefreshToken = context.RefreshToken,
        }, context.HttpContext.RequestAborted);
        if (!success)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Failed to save token for {patreonId}", patreonId);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddHostedService<TokenRefresher>();
builder.Services.AddTransient<RefreshTokenContext>();
builder.Services.AddSingleton<ITokenDatabase, TokenDatabase>();
builder.Services.AddHttpClient("patreon-refresh", client =>
    client.Timeout = TimeSpan.FromSeconds(10));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (ClaimsPrincipal user) =>
    Results.Ok(user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.MapGet("/login", () =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, new List<string> { "patreon" }));

app.Run();
