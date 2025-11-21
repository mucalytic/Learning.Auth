var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/username", (HttpContext context) => // before we can get a username, we need to sign in. we need to issue a cookie.
{
    var authCookie = context.Request.Headers.Cookie.FirstOrDefault(s => s is not null && s.StartsWith("auth="));
    var payload = authCookie?.Split('=').LastOrDefault();
    var username = payload?.Split(':').LastOrDefault();
    return username ?? "empty";
});

app.MapGet("/login", (HttpContext context) => // HttpContext contains all request headers, url, etc. throughout the lifetime of the request and response.
{
    context.Response.Headers["set-cookie"] = "auth=user:aaron";
    return "ok";
});

app.Run();
