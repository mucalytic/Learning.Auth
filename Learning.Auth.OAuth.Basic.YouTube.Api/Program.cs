using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie").AddCookie("cookie");

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
    Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("user_id", Guid.NewGuid().ToString())], "cookie")),
        authenticationScheme: "cookie"));

app.Run();
