using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("jwt").AddJwtBearer("jwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Query.ContainsKey("t"))
            {
                context.Token = context.Request.Query["t"];
            }
            return Task.CompletedTask;
        }
    };
    options.Configuration = new OpenIdConnectConfiguration
    {
        SigningKeys = {  }
    };
    options.MapInboundClaims = false;
});

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext context) =>
    context.User.FindFirst("subscriptionId")?.Value ?? "empty");

app.MapGet("/create-jwt-token", (string rsaPrivateKey) =>
{
    var rsa = RSA.Create();
    var bytes = Convert.FromBase64String(rsaPrivateKey);
    rsa.ImportRSAPrivateKey(bytes, out _);
    var key = new RsaSecurityKey(rsa);
    var handler = new JsonWebTokenHandler();
    var token = handler.CreateToken(
        new SecurityTokenDescriptor
        {
            Issuer = "https://localhost:7199",
            Subject = new ClaimsIdentity([
                new Claim("subscriptionId", Guid.NewGuid().ToString()),
                new Claim("name", "aaron")
            ]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        });
    return token;
});

app.Run();
