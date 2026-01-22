var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddOAuth("github", options =>
{
    options.ClientId = "Ov23liGvE0Z5ZMfIjjnL";
    options.ClientSecret = "3eca3e78147ff5d9e11e9bbe31c76434d5a537e2";
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.CallbackPath = "/oauth/callback";
});

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
    Results.Challenge(authenticationSchemes: ["github"]));

app.MapGet("/", (HttpContext context) =>
    Results.Ok(context.User.Claims.Select(claim => new { claim.Type, claim.Value })));

app.Run();
