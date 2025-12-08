// https://youtu.be/hw2B6SZj8y8?si=tmi8-mft3bYkSgvQ&t=476

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("default") // set default scheme so that you don't have to specify authorisation policies
                .AddCookie("default", options =>
                {
                    options.Cookie.Name = "aaron.cookie";
                    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                });
builder.Services.AddAuthorization();
builder.Services.AddControllers(); // allows you to pick up controllers from the file directory

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapGet("/", () => "hello");
app.MapGet("/test", () => "hello")
   .RequireAuthorization();
app.MapPost("/login", async (HttpContext httpContext) =>
{
    IEnumerable<Claim> claims = [
        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) // userId
    ];
    var identity = new ClaimsIdentity(claims, "default"); // authentication type can be anything
    var user = new ClaimsPrincipal(identity);
    await httpContext.SignInAsync("default", user, new AuthenticationProperties { IsPersistent = true }); // must match the registered authentication (cookie) scheme
    return "ok";
});
app.MapDefaultControllerRoute();
app.Run();
