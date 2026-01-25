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
                        /* 1. navigating to / checks "youtube-enabled" policy
                         * 2. that requires authentication under the "cookie" scheme
                         * 3. so it redirects to the /login endpoint which sets a cookie containing user details
                         * 4. then it redirects back to / which again checks the "youtube-enabled" policy
                         * 5. that policy also requires that the user has the "youtube-token" claim
                         * 6. because they don't, so it redirects them to the access denied page
                         * 7. we catch it here and use oath with youtube
                         */
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

app.MapGet("/", (IHttpClientFactory factory, HttpContext context, Database database) =>
{
    var user = context.User;
    var userId = user.FindFirstValue("user_id");
    if (userId is null) return Results.Unauthorized();
    var accessToken = database[userId];
    var client = factory.CreateClient();
    return Results.Ok();
})
.RequireAuthorization("youtube-enabled"); // not allowed to visit this endpoint without a "youtube-token" claim

app.Run();
