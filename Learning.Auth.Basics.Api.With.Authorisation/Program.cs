using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

const string authScheme = "cookie";

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddAuthentication(authScheme)
       .AddCookie(authScheme);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();

app.Use((context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/login")) return next(context); // allow anonymous
    if (context.User.Identities.All(identity => identity.AuthenticationType != authScheme))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized; // not authenticated
        return Task.CompletedTask;
    }
    if (!context.User.HasClaim("pass", "ch"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden; // not authorised
        return Task.CompletedTask;
    }
    return next(context);
});

// this will only return the name if you've logged in and have a "pass" claim with a value of "ch"
app.MapGet("/unsecure", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value ?? "empty");

app.MapGet("/login", async context =>
{
    IEnumerable<Claim> claims = [
        new("usr", "aaron"),
        new("pass", "ch")
    ];
    var identity = new ClaimsIdentity(claims, authScheme);
    var user = new ClaimsPrincipal(identity);
    // the user is specifically signing in with the schema "cookie"
    await context.SignInAsync(authScheme, user);
});

app.Run();
