using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("visiting-russia", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
                     .AddAuthenticationSchemes("cookie")
                     .RequireClaim("passport")
                     .RequireClaim("visa", "russia");
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/username", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value ?? "empty")
   .AllowAnonymous();

app.MapGet("/sign-in", async (HttpContext context) =>
    {
        var claims = new List<Claim>
        {
            new("usr", "aaron"),
            new("passport", "uk"),
            new("visa", "ukraine")
        };
        var identity = new ClaimsIdentity(claims, "cookie");
        var user = new ClaimsPrincipal(identity);
        await context.SignInAsync("cookie", user);
        return "Ok";
    })
   .AllowAnonymous();

app.MapGet("/visiting/russia", () => "Welcome to Russia!")
   .RequireAuthorization("visiting-russia");

app.Run();
