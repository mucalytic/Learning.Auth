using static Microsoft.IdentityModel.Tokens.SecurityAlgorithms;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;

// this reads the RSA key pair from the file created in the KeyGen project
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

app.UseHttpsRedirection();

app.MapGet("/", (HttpContext ctx) => "hello world");
app.MapGet("/jwt", () =>
{
    // this uses the RSA private key to sign the JWT token
    var key = new RsaSecurityKey(rsaKey);
    var handler = new JsonWebTokenHandler();
    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = "https://localhost:7199/",
        Subject = new ClaimsIdentity([
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "Aaron")
        ]),
        SigningCredentials = new SigningCredentials(key, RsaSha256)
    };
    var token = handler.CreateToken(descriptor);
    return token; // returns a JWT token
});

app.Run();
