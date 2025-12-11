using static System.Security.Claims.ClaimTypes;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie").AddCookie("cookie");

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/login", () =>
{
    IEnumerable<Claim> claims = [new(NameIdentifier, Guid.NewGuid().ToString())];
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);
    return Results.SignIn(user, new AuthenticationProperties(), "cookie");
});
app.MapGet("/user", (ClaimsPrincipal user) =>
    user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList());
app.Run();
