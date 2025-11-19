using static Microsoft.IdentityModel.Tokens.SecurityAlgorithms;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// this reads the RSA key pair from the file created in the KeyGen project
var rsaKey = RSA.Create();
var privateKey = File.ReadAllBytes("key");
rsaKey.ImportRSAPrivateKey(privateKey, out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("jwt") // ← sets the DEFAULT scheme to "jwt". any [Authorize] attribute without an explicit scheme will now use the "jwt" scheme.
    .AddJwtBearer("jwt", options => // ← registers the JwtBearer handler under the name "jwt" (instead of the built-in "Bearer").
    {
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
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication(); // calls JwtBearerHandler.HandleAuthenticateAsync()

app.MapGet("/", (HttpContext context) => context.User.FindFirst("sub"));

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
