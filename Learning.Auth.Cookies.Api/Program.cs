var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/username", (HttpContext context) =>
{
    var username =
       (from cookie in context.Request.Headers["cookie"]
        where cookie.StartsWith("auth=")
        let parts = cookie.Split('=').Last()
        where parts.StartsWith("usr:")
        select parts.Split(':').Last())
       .FirstOrDefault();
    return username;
});

app.MapGet("/sign-in", (HttpContext context) =>
{
    context.Response.Headers["set-cookie"] = "auth=usr:aaron";
    return "Ok";
});

app.Run();
