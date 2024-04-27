using Default = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Learning.Auth.Identity.Cookies.Api;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddAuthentication(Default.AuthenticationScheme)
                .AddCookie(Default.AuthenticationScheme);
builder.Services.AddSingleton<IDictionary<string, User>>(new Dictionary<string, User>());
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("developers-only", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
                     .AddAuthenticationSchemes(Default.AuthenticationScheme)
                     .RequireClaim("role", "developer")
                     .Build();
    });
    options.AddPolicy("signed-in", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
                     .AddAuthenticationSchemes(Default.AuthenticationScheme)
                     .Build();
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/register",
    async (IPasswordHasher<User> hasher, IDictionary<string, User> store,
           HttpContext context, string username, string password) =>
    {
        var user = new User { Username = new Username(username) };
        user = user with { PasswordHash = hasher.HashPassword(user, password) };
        await context.SignInAsync(
            Default.AuthenticationScheme,
            user.ToClaimsPrincipal(Default.AuthenticationScheme));
        return store.TryAdd(username.Hash(), user)
            ? Results.Ok(user) : Results.Conflict();
    })
   .AllowAnonymous();

app.MapGet("/sign-in",
    async (IPasswordHasher<User> hasher, IDictionary<string, User> store,
           HttpContext context, string username, string password) =>
    {
        if (!store.TryGetValue(username.Hash(), out var user)) return Results.NotFound();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed) return Results.Forbid();
        await context.SignInAsync(
            Default.AuthenticationScheme,
            user.ToClaimsPrincipal(Default.AuthenticationScheme));
        return Results.Ok(user);
    })
   .AllowAnonymous();

app.MapGet("/employ",
   (IDictionary<string, User> store, string username) =>
    {
        var usernameHash = username.Hash();
        if (!store.TryGetValue(usernameHash, out var user)) return Results.NotFound();
        if (!user.Claims.TryAdd("role", "developer")) return Results.Conflict();
        store[usernameHash] = user;
        return Results.Ok(user);
    })
   .AllowAnonymous();

app.MapGet("/fix-bugs", () => "Good job!")
   .RequireAuthorization("developers-only");

app.MapGet("/start-password-reset",
   (IDictionary<string, User> store, IDataProtectionProvider provider, string username) =>
        store.TryGetValue(username.Hash(), out var user)
            ? Results.Ok(provider.CreateProtector("password-reset")
                                 .Protect(user.Username.Value))
            : Results.NotFound())
   .RequireAuthorization("signed-in");

app.MapGet("/end-password-reset",
   (IDictionary<string, User> store, IDataProtectionProvider provider,
    IPasswordHasher<User> hasher, string username, string password, string hash) =>
    {
        var unhashedUsername = provider.CreateProtector("password-reset").Unprotect(hash);
        if (unhashedUsername != username) return Results.BadRequest();
        var usernameHash = username.Hash();
        if (!store.TryGetValue(usernameHash, out var user)) return Results.NotFound();
        user = user with { PasswordHash = hasher.HashPassword(user, password) };
        store[usernameHash] = user;
        return Results.Ok(user);
    })
   .AllowAnonymous();

app.Run();
