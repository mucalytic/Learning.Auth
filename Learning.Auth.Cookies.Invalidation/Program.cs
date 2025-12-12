using Learning.Auth.Cookies.Invalidation.Interfaces;
using Learning.Auth.Cookies.Invalidation.Blacklists;
using static System.Security.Claims.ClaimTypes;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITokenBlacklist, RedisTokenBlacklist>();
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
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = "localhost:6379");

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/login", async (HttpContext context) =>
{
    IEnumerable<Claim> claims = [
        new(NameIdentifier, Guid.NewGuid().ToString()),
        new("session", Guid.NewGuid().ToString())
    ];
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);
    var properties = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
    };
    await context.SignInAsync("cookie", user, properties);
    return Results.Ok("logged in");
});
app.MapGet("/logout", async (HttpContext context, ITokenBlacklist blacklist) =>
{
    var session = context.User.FindFirstValue("session");
    if (session is not null)
    {
        var expiry = context.User.FindFirstValue(Expiration);
        if (expiry is not null)
        {
            await blacklist.BlacklistAsync(session, DateTimeOffset.Parse(expiry));
        }
    }
    await context.SignOutAsync("cookie");
    return Results.Ok("logged out");
});
app.MapGet("/user", (ClaimsPrincipal user) =>
    user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList());
app.Run();
