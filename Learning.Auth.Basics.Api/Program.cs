using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection(); // makes an IDataProtectionProvider available in DI

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/username", (HttpContext context, IDataProtectionProvider provider) => // before we can get a username, we need to sign in. we need to issue a cookie.
{
    var protector = provider.CreateProtector("auth-cookie");
    var authCookie = context.Request.Headers.Cookie.FirstOrDefault(s => s is not null && s.StartsWith("auth="));
    var protectedPayload = authCookie?.Split('=').LastOrDefault();
    if (protectedPayload is null) return "empty";
    var payload = protector.Unprotect(protectedPayload);
    var username = payload.Split(':').LastOrDefault();
    return username ?? "empty";
});

app.MapGet("/login", (HttpContext context, IDataProtectionProvider provider) => // HttpContext contains all request headers, url, etc. throughout the lifetime of the request and response.
{
    var protector = provider.CreateProtector("auth-cookie");
    context.Response.Headers.SetCookie = $"auth={protector.Protect("user:aaron")}";
    return "ok";
});

app.Run();
