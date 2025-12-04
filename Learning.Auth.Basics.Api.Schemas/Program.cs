using Microsoft.AspNetCore.Authentication.Cookies;
using Learning.Auth.Basics.Api.Schemas.Handlers;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// https://youtu.be/N_zVCCpnjXM?si=ZdB4IQy6jxJps_aF

builder.Services.AddAuthentication()
                .AddScheme<CookieAuthenticationOptions, VisitorAuthenticationHandler>("visitor", _ => { })
                .AddCookie("local");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("customer", policy =>
    {
        policy.AddAuthenticationSchemes("local", "visitor")
              .RequireAuthenticatedUser();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "hello")
   .RequireAuthorization("customer");

app.MapGet("/login-local", async context => {
        IEnumerable<Claim> claims = [new("usr", "aaron")];
        var identity = new ClaimsIdentity(claims, "local");
        var user = new ClaimsPrincipal(identity);
        await context.SignInAsync("local", user);
    })
   .AllowAnonymous();

app.Run();
