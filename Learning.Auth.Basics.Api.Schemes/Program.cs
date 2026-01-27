using Microsoft.AspNetCore.Authentication.Cookies;
using Learning.Auth.Basics.Api.Schemes.Handlers;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// video:
// https://youtu.be/N_zVCCpnjXM?si=ZdB4IQy6jxJps_aF

builder.Services.AddAuthentication()
                .AddScheme<CookieAuthenticationOptions, VisitorAuthenticationHandler>("visitor", _ => { }) // give a visitor a token no questions asked
                .AddCookie("local")
                .AddCookie("wiremock")
                .AddOAuth("external-wiremock", options => // https://docs.wiremock.io/security/oauth2-mock
                {
                    options.SignInScheme = "wiremock";
                    options.ClientId = "id"; // username
                    options.ClientSecret = "secret"; // password
                    options.AuthorizationEndpoint = "https://oauth.wiremockapi.cloud/oauth/authorize";
                    options.TokenEndpoint = "https://oauth.wiremockapi.cloud/oauth/token";
                    options.UserInformationEndpoint = "https://oauth.wiremockapi.cloud/userinfo";
                    options.CallbackPath = "/callback-wiremock";
                    options.Scope.Add("profile");
                    options.SaveTokens = true;
                });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("customer", policy =>
    {
        policy.AddAuthenticationSchemes("local", "wiremock", "visitor")
              .RequireAuthenticatedUser();
    });
    options.AddPolicy("user", policy =>
    {
        policy.AddAuthenticationSchemes("local")
              .RequireAuthenticatedUser();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) => Results.Ok("hello"))
   .RequireAuthorization("customer");

app.MapGet("/login-local", async context => {
        IEnumerable<Claim> claims = [new("usr", "aaron")];
        var identity = new ClaimsIdentity(claims, "local");
        var user = new ClaimsPrincipal(identity);
        await context.SignInAsync("local", user);
    })
    .AllowAnonymous();

app.MapGet("/login-wiremock", async context => await context.ChallengeAsync("external-wiremock", new AuthenticationProperties { RedirectUri = "/" }))
   .RequireAuthorization("user"); // use the policy "user", which means we need to be authenticated with the "local" scheme NOT "visitor"

app.Run();
