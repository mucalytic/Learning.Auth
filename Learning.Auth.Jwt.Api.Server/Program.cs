using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var rsaKey = RSA.Create();
var privateKey = File.ReadAllBytes("key");
rsaKey.ImportRSAPrivateKey(privateKey, out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("jwt")
    .AddJwtBearer("jwt", options =>
    {
        
    });

var app = builder.Build();

app.MapGet("/", (HttpContext ctx) => "hello world");
app.MapGet("/jwt", () =>
{
    var key = new RsaSecurityKey(rsaKey);
    var handler = new JsonWebTokenHandler();
    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = "https://localhost:5001/",
        Subject = new ClaimsIdentity([
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "Aaron")
        ]),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    };
    var token = handler.CreateToken(descriptor);
    return token;
});

app.Run();
