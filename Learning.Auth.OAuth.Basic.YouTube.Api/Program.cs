using System.Net.Http.Headers;
using Learning.Auth.OAuth.Basic.YouTube.Api;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OAuthOptions>("youtube", builder.Configuration.GetSection("youtube"));
builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie", options =>
                {
                    var action = options.Events.OnRedirectToAccessDenied;
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path == "/")
                        {
                            return context.HttpContext.ChallengeAsync("youtube");
                        }
                        return action(context);
                    };
                    options.LoginPath = "/login";
                })
                .AddOAuth("youtube", options =>
                {
                    options.ClientId = builder.Configuration["youtube:clientId"] ?? string.Empty;
                    options.ClientSecret = builder.Configuration["youtube:clientSecret"] ?? string.Empty;
                    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                    options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                    options.CallbackPath = "/oauth/callback";
                    options.SignInScheme = "cookie";
                    options.SaveTokens = false;
                    options.Scope.Clear();
                    options.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");
                    options.Events.OnCreatingTicket = async context =>
                    {
                        var ahp = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                        var handler = await ahp.GetHandlerAsync(context.HttpContext, "cookie");
                        if (handler is null)
                        {
                            context.Fail("cookie handler not found");
                            return;
                        }
                        var result = await handler.AuthenticateAsync();
                        if (!result.Succeeded)
                        {
                            context.Fail("cookie authentication failed");
                            return;
                        }
                        var userId = result.Principal.FindFirstValue("user_id");
                        if (userId is null)
                        {
                            context.Fail("user_id claim not found");
                            return;
                        }
                        var db = context.HttpContext.RequestServices.GetRequiredService<Database>();
                        if (context.AccessToken is null)
                        {
                            context.Fail("access token not found");
                            return;
                        }
                        db[userId] = context.AccessToken;
                        context.Principal = result.Principal.Clone();
                        var identity = context.Principal.Identities.FirstOrDefault(id => id.AuthenticationType == "cookie");
                        if (identity is null)
                        {
                            context.Fail("cookie identity not found");
                            return;
                        }
                        identity.AddClaim(new Claim("youtube-token", "yes"));
                    };
                });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("youtube-enabled", policy =>
    {
        policy.AddAuthenticationSchemes("cookie")
              .RequireClaim("youtube-token", "yes")
              .RequireAuthenticatedUser();
    });
});
builder.Services.AddScoped<IClaimsTransformation, YouTubeTokenClaimsTransformation>();
builder.Services.AddSingleton<Database>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", () =>
    Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("user_id", Guid.NewGuid().ToString())], "cookie")),
        authenticationScheme: "cookie"));

app.MapGet("/", async (IHttpClientFactory httpClientFactory, HttpContext context) =>
{
    const string uri = "https://www.googleapis.com/youtube/v3/channels?part=snippet&mine=true";
    var accessToken = context.User.FindFirstValue("youtube-access-token");
    if (accessToken is null) return Results.Unauthorized();
    using var request = new HttpRequestMessage(HttpMethod.Get, uri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var client = httpClientFactory.CreateClient();
    using var response = await client.SendAsync(request);
    var info = await response.Content.ReadAsStringAsync();
    return Results.Ok(info);
})
.RequireAuthorization("youtube-enabled");

app.Run();
