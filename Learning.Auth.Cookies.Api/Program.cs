using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie");

var app = builder.Build();

app.UseAuthentication();

app.Use((context, next) =>
{
    if (context.User.Identities.All(i => i.AuthenticationType != "cookie"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
    if (!context.User.HasClaim("passport", "russia")) // the required claim comes from attribute or extension on the endpoint
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
    return next();
});

app.MapGet("/username", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value ?? "empty");

app.MapGet("/sign-in", async (HttpContext context) =>
    {
        var claims = new List<Claim>
        {
            new("usr", "aaron"),
            new("passport", "uk"),
            new("passport", "russia")
        };
        var identity = new ClaimsIdentity(claims, "cookie");
        var user = new ClaimsPrincipal(identity);
        await context.SignInAsync("cookie", user);
        return "Ok";
    })
   .AllowAnonymous();

app.MapGet("/visiting/russia", () => { });

app.Run();
