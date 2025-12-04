using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie").AddCookie("cookie");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();

app.MapGet("/username", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value ?? "empty");

// logic for dealing out the authentication cookie
app.MapGet("/login", async (HttpContext context) =>
{
    // just here you log the person in with their
    // credentials, check a db and create real claims
    IEnumerable<Claim> claims = [new("usr", "aaron")];
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);
    await context.SignInAsync("cookie", user);
    return "ok";
});

app.Run();
