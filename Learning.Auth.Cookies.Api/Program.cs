using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie").AddCookie("cookie", options =>
{
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("visiting-russia", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
                     .AddAuthenticationSchemes("cookie")
                     .RequireClaim("passport")
                     .RequireClaim("visa", "russia")
                     .Build();
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
            new("visa", "russia")
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
