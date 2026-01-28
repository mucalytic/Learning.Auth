using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Entities;
using Learning.Auth.OAuth.Refresh.Tokens.Services;
using Microsoft.AspNetCore.Authentication;
using Learning.Auth.OAuth.Refresh.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OAuthOptions>("patreon", builder.Configuration.GetSection("patreon"));
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
        var database = context.HttpContext.RequestServices.GetRequiredService<ITokenDatabase>();
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        var patreonId = json.GetProperty("data").GetProperty("id").GetString();
        if (patreonId is null) return;
        await database.TrySaveAsync(patreonId,
            new TokenInfo(context.AccessToken, context.RefreshToken, context.ExpiresIn));
        context.Identity?.AddClaim(new Claim("patreonId", patreonId));
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<ITokenDatabase, TokenDatabase>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (ClaimsPrincipal user) =>
    Results.Ok(user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.MapGet("/login", () =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, new List<string> { "patreon" }));

app.Run();
