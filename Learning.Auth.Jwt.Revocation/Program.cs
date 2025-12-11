using Learning.Auth.Jwt.Revocation.Blacklists;
using Learning.Auth.Jwt.Revocation.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/", () => "hello");
app.Run();
