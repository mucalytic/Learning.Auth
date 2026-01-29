using Learning.Auth.OAuth.Refresh.Tokens.EventHandlers;
using Learning.Auth.OAuth.Refresh.Tokens.Interfaces;
using Learning.Auth.OAuth.Refresh.Tokens.Background;
using Learning.Auth.OAuth.Refresh.Tokens.Services;
using Microsoft.AspNetCore.Authentication;
using Learning.Auth.OAuth.Refresh.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
        var handler = context.HttpContext.RequestServices.GetRequiredService<PatreonCreatingTicketHandler>();
        await handler.HandleAsync(context);
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHostedService<TokenRefresher>();
builder.Services.AddScoped<RefreshTokenContext>();
builder.Services.AddScoped<PatreonCreatingTicketHandler>();
builder.Services.AddScoped<ITokenDatabase, TokenDatabase>();
builder.Services.Configure<OAuthConfig>("patreon", builder.Configuration.GetSection("patreon"));
builder.Services.AddHttpClient("patreon-refresh", client => client.Timeout = TimeSpan.FromSeconds(10));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (ClaimsPrincipal user) =>
    Results.Ok(user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.MapGet("/login", () =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, new List<string> { "patreon" }));

app.Run();
