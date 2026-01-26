using Learning.Auth.OAuth.Custom.Server.Endpoints.OAuth;
using Learning.Auth.OAuth.Custom.Server.Endpoints;
using Learning.Auth.OAuth.Custom.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
                .AddCookie("cookie", options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DevKeysService>();
builder.Services.AddScoped<ValidationCodeVerifier>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login",           GetLogin.Handler);
app.MapPost("/login",          PostLogin.Handler);
app.MapPost("/oauth/token",    PostToken.Handler);
app.MapGet("/oauth/authorize", GetAuthorize.Handler).RequireAuthorization();

app.Run();
