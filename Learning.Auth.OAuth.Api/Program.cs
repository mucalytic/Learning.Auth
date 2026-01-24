using Learning.Auth.OAuth.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OAuthOptions>("github", builder.Configuration.GetSection("github"));

// This stuff only works on .NET 6.
builder.Services.AddAuthentication().AddOAuth("github", options =>
{
    options.ClientSecret = builder.Configuration["github:clientSecret"] ?? string.Empty;
    options.ClientId = builder.Configuration["github:clientId"] ?? string.Empty;
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.CallbackPath = "/oauth/callback";
});

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
    Results.Challenge(authenticationSchemes: new List<string> {"github"}));

app.MapGet("/", (HttpContext context) =>
    Results.Ok(context.User.Claims.Select(claim => new { claim.Type, claim.Value }).ToList()));

app.Run();
