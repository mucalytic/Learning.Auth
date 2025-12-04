using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
                .AddCookie("visitor")
                .AddCookie("local");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();

app.MapGet("/login-local", async context =>
{
    IEnumerable<Claim> claims = [new("usr", "aaron")];
    var identity = new ClaimsIdentity(claims, "local");
    var user = new ClaimsPrincipal(identity);
    await context.SignInAsync("local", user);
});

app.Run();
