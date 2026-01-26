using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

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
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = 
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    options.Events.OnCreatingTicket = context =>
                    {
                        if (context.AccessToken is null) return Task.CompletedTask;
                        var payloadBase64 = context.AccessToken.Split('.')[1];
                        var payloadJson = Base64UrlTextEncoder.Decode(payloadBase64);
                        var payload = JsonDocument.Parse(payloadJson);
                        context.RunClaimActions(payload.RootElement);
                        return Task.CompletedTask;
                    };
                    options.ClaimActions.MapJsonKey("sub", "sub");
                    options.ClaimActions.MapJsonKey("bar", "custom");
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
