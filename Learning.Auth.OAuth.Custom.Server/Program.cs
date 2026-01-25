using Learning.Auth.OAuth.Custom.Server.Endpoints.OAuth;
using Learning.Auth.OAuth.Custom.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie", options => options.LoginPath = "/login");

builder.Services.AddAuthorization();
builder.Services.AddSingleton<object>();

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login",           GetLogin.Handler);
app.MapPost("/login",          PostLogin.Handler);
app.MapPost("/oauth/token",    PostToken.Handler);
app.MapGet("/oauth/authorize", GetAuthorize.Handler)
   .RequireAuthorization();

app.Run();
