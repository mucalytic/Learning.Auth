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

app.MapGet("/switzerland", (HttpContext context) =>
{
    // first must be authenticated with the correct schema ("cookie")
    if (context.User.Identities.All(identity => identity.AuthenticationType != authScheme))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return "unauthorised";
    }
    // then must have the correct claim with the correct value
    if (context.User.HasClaim("pass", "ch")) return "allowed";
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    return "not allowed";
}); 

app.MapGet("/login", async context =>
{
    IEnumerable<Claim> claims = [
        new("usr", "aaron"),
        new("pass", "ch")
    ];
    var identity = new ClaimsIdentity(claims, authScheme);
    var user = new ClaimsPrincipal(identity);
    await context.SignInAsync(authScheme, user);
});

app.Run();
