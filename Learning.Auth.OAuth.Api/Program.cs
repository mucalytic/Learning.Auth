using Microsoft.AspNetCore.Authentication;
using Learning.Auth.OAuth.Api;

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
});

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, new List<string> {"github"}));

app.MapGet("/", (HttpContext context) =>
    Results.Ok(context.User.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.Run();
