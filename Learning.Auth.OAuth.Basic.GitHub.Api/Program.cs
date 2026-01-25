using Microsoft.AspNetCore.Authentication;
using Learning.Auth.OAuth.Basic.GitHub.Api;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OAuthOptions>("github", builder.Configuration.GetSection("github"));

builder.Services.AddAuthentication("cookie").AddCookie("cookie").AddOAuth("github", options =>
{
    options.ClientSecret = builder.Configuration["github:clientSecret"] ?? string.Empty;
    options.ClientId = builder.Configuration["github:clientId"] ?? string.Empty;
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.CallbackPath = "/oauth/callback";
    options.SignInScheme = "cookie";
    options.Events.OnCreatingTicket = async context =>
    {
        // should store the access token somewhere so can use the refresh token to get another token later
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request);
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        context.RunClaimActions(user);
    };
    options.ClaimActions.MapJsonKey("sub", "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
});

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, new List<string> {"github"}));

app.MapGet("/", (HttpContext context) =>
    Results.Ok(context.User.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.Run();
