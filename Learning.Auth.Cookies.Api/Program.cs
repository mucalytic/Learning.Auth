using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie");

var app = builder.Build();

app.UseAuthentication();

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
});

app.MapGet("/visiting/russia", (HttpContext context) =>
{
    if (context.User.Identities.All(i => i.AuthenticationType != "cookie"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return "not authenticated";
    }
    if (!context.User.HasClaim("passport", "russia"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return "not authorised";
    }
    return "allowed";
});

app.Run();
