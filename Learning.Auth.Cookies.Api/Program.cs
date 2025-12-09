// https://youtu.be/hw2B6SZj8y8?si=PG97C6buNHtQu9uB&t=2293

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("default") // set default scheme so that you don't have to specify authorisation policies
                .AddCookie("default", options =>
                {
                    options.Cookie.Name = "aaron.cookie";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("failure", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("blah", "fnar");
    });
});
builder.Services.AddControllers(); // allows you to pick up controllers from the file directory

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapGet("/", () => "hello");
app.MapGet("/test", () => "hello")
   .RequireAuthorization("failure");
app.MapPost("/login", async (HttpContext httpContext) =>
{
    IEnumerable<Claim> claims = [
        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) // userId
    ];
    var identity = new ClaimsIdentity(claims, "default"); // authentication type can be anything
    var user = new ClaimsPrincipal(identity);
    var properties = new AuthenticationProperties { IsPersistent = true };
    await httpContext.SignInAsync("default", user, properties); // must match the registered authentication (cookie) scheme
    return "ok";
});
app.MapGet("/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync("default");
    return "ok";
});
app.MapDefaultControllerRoute();
app.Run();
