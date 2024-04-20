using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie");

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/username", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value);

app.MapGet("/sign-in", async (HttpContext context) =>
{
    var claims = new List<Claim> { new("usr", "aaron") };
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);
    await context.SignInAsync("cookie", user);
    return "Ok";
});

app.Run();
