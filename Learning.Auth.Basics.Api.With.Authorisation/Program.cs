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

app.MapGet("/unsecure", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value ?? "empty");

app.MapGet("/login", async (HttpContext context) =>
{
    IEnumerable<Claim> claims = [new("usr", "aaron")];
    var identity = new ClaimsIdentity(claims, authScheme);
    var user = new ClaimsPrincipal(identity);
    await context.SignInAsync(authScheme, user);
});

app.Run();
