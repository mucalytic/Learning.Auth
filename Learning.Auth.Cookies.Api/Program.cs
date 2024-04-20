using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

var app = builder.Build();

app.MapGet("/username", (HttpContext context, IDataProtectionProvider provider) =>
{
    var protector = provider.CreateProtector("auth-cookie");
    var username =
       (from cookie in context.Request.Headers["cookie"]
        where cookie.StartsWith("auth=")
        let encoded = cookie.Split('=').Last()
        let parts = protector.Unprotect(encoded)
        where parts.StartsWith("usr:")
        select parts.Split(':').Last())
       .FirstOrDefault();
    return username;
});

app.MapGet("/sign-in", (HttpContext context, IDataProtectionProvider provider) =>
{
    var protector = provider.CreateProtector("auth-cookie");
    var cookie = $"auth={protector.Protect("usr:aaron")}";
    context.Response.Headers["set-cookie"] = cookie;
    return "Ok";
});

app.Run();
