using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie")
                .AddOAuth("custom", options =>
                {
                    options.SignInScheme = "cookie";
                    options.ClientId = "x";
                    options.ClientSecret = "x";
                    options.AuthorizationEndpoint = "https://localhost:5005/oauth/authorize";
                    options.TokenEndpoint = "https://localhost:5005/oauth/token";
                    options.CallbackPath = "/oauth/callback";
                    options.UsePkce = true;
                    options.ClaimActions.MapJsonKey("sub", "sub");
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = 
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
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
            RedirectUri = "https://localhost:5004/"
        },
        authenticationSchemes: ["custom"]));

app.Run();
