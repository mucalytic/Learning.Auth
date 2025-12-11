using Learning.Auth.Cookies.Invalidation.Interfaces;
using Learning.Auth.Cookies.Invalidation.Blacklists;
using static System.Security.Claims.ClaimTypes;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();
builder.Services.AddAuthentication("cookie").AddCookie("cookie", options =>
{
    options.Events.OnValidatePrincipal = async context =>
    {
        var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();
        var session = context.Principal?.FindFirstValue("session");
        if (session is null) return;
        var blacklisted = await blacklist.IsBlacklistedAsync(session);
        if (blacklisted) context.RejectPrincipal();
    };
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/login", () =>
{
    IEnumerable<Claim> claims = [
        new(NameIdentifier, Guid.NewGuid().ToString()),
        new("session", Guid.NewGuid().ToString())
    ];
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);
    return Results.SignIn(user, new AuthenticationProperties(), "cookie");
});
app.MapGet("/user", (ClaimsPrincipal user) =>
    user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList());
app.MapGet("/blacklist", async (ITokenBlacklist blacklist, string session) =>
    await blacklist.BlacklistAsync(session, DateTime.UtcNow.AddMinutes(10)));
app.Run();
