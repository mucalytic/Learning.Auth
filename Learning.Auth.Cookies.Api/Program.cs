using Microsoft.AspNetCore.DataProtection;
using Learning.Auth.Cookies.Api;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.Use((context, next) =>
{
    var provider = context.RequestServices.GetRequiredService<IDataProtectionProvider>();
    var protector = provider.CreateProtector("auth-cookie");
    var (key, value) =
        (from cookie in context.Request.Headers["cookie"]
         where cookie.StartsWith("auth=")
         let encoded = cookie.Split('=').Last()
         let payload = protector.Unprotect(encoded)
         where payload.StartsWith("usr:")
         let parts = payload.Split(':').Take(2)
         select (parts.First(), parts.Last()))
       .FirstOrDefault();
    var claims = new List<Claim> { new(key, value) };
    var identity = new ClaimsIdentity(claims);
    context.User = new ClaimsPrincipal(identity);
    return next();
});

app.MapGet("/username", (HttpContext context) =>
    context.User.FindFirst("usr")?.Value);

app.MapGet("/sign-in", (AuthService authService) =>
{
    authService.SignIn();
    return "Ok";
});

app.Run();
