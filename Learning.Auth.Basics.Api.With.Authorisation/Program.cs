using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

const string authScheme = "cookie";

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddAuthentication(authScheme)
       .AddCookie(authScheme);

builder.Services
       .AddAuthorization(options =>
       {
           options.AddPolicy("swiss-passport", policy =>
           {
               policy.RequireAuthenticatedUser()
                     .AddAuthenticationSchemes(authScheme)
                     .RequireClaim("pass", "ch");
           });
       });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// this will only return the name if you've logged in and have a "pass" claim with a value of "ch"
app.MapGet("/unsecure", (HttpContext context) => context.User.FindFirst("usr")?.Value ?? "empty")
   .RequireAuthorization("swiss-passport");

app.MapGet("/login", async context => {
        IEnumerable<Claim> claims = [
            new("usr", "aaron"),
            new("pass", "ch")
        ];
        var identity = new ClaimsIdentity(claims, authScheme);
        var user = new ClaimsPrincipal(identity);
        // the user is specifically signing in with the schema "cookie"
        await context.SignInAsync(authScheme, user);
    })
   .AllowAnonymous();

app.Run();
