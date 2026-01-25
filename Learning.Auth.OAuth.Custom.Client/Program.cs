using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie")
                .AddOAuth("custom", options =>
                {
                    options.SignInScheme = "cookie";
                    options.ClientId = "";
                    options.ClientSecret = "";
                    options.AuthorizationEndpoint = "https://localhost:7045/oauth/authorize";
                    options.TokenEndpoint = "https://localhost:7045/oauth/token";
                    options.CallbackPath = "/oauth/callback";
                    options.UsePkce = true;
                    options.ClaimActions.MapJsonKey("sub", "sub");
                    options.Events.OnCreatingTicket = context => Task.CompletedTask;
                });

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext context) =>
    context.User.Claims.Select(claim => new { claim.Type, claim.Value }).ToList());

app.MapGet("/login", () =>
    Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = "https://localhost:7131/"
        },
        authenticationSchemes: ["custom"]));

app.Run();
