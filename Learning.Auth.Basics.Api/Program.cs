using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Learning.Auth.Basics.Api.Interfaces;
using Learning.Auth.Basics.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection(); // makes an IDataProtectionProvider available in DI
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var app = builder.Build();

app.Use((context, next) =>
{
    using var scope = context.RequestServices.CreateScope();
    var provider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
    var protector = provider.CreateProtector("auth-cookie");
    var authCookie = context.Request.Headers.Cookie.FirstOrDefault(s => s is not null && s.StartsWith("auth="));
    var protectedPayload = authCookie?.Split('=').LastOrDefault();
    if (protectedPayload is null) return next(context);
    var payload = protector.Unprotect(protectedPayload);
    var pair = payload.Split(':');
    var (key, val) = (pair.First(), pair.Last());
    var claim = new Claim(key, val);
    var identity = new ClaimsIdentity([claim]);
    context.User = new ClaimsPrincipal(identity);
    return next(context);
});
app.UseHttpsRedirection();

app.MapGet("/username", (HttpContext context) => // before we can get a username, we need to sign in. we need to issue a cookie.
{
    return context.User.FindFirst("usr")?.Value ?? "empty";
});

app.MapGet("/login", (IAuthenticationService authenticationService) =>
{
    authenticationService.SignIn();
    return "ok";
});

app.Run();
