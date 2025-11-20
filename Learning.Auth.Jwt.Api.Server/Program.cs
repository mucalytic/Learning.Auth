using static Microsoft.IdentityModel.Tokens.SecurityAlgorithms;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

// this reads the RSA key pair from the file created in the KeyGen project
var rsaKey = RSA.Create();
var privateKey = File.ReadAllBytes("key");
rsaKey.ImportRSAPrivateKey(privateKey, out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("jwt") // ← sets the DEFAULT scheme to "jwt". any [Authorize] attribute without an explicit scheme will now use the "jwt" scheme.
    .AddJwtBearer("jwt", options => // ← registers the JwtBearer handler under the name "jwt" (instead of the built-in "Bearer").
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
            SigningKeys =
            {
                new RsaSecurityKey(rsaKey) // ← this is the public and private key pair. it can create AND verify a signature.
            }
        };
        options.MapInboundClaims = false; // ← this is to ensure that the "sub" claim exists, instead of some replacement that microsoft injects instead
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication(); // calls JwtBearerHandler.HandleAuthenticateAsync()

app.MapGet("/", (HttpContext context) => // calling this endpoint calls the event handler above, specified in OnMessageReceived and grabs the token from the query string
{
    var claim = context.User.FindFirst("sub");
    return claim?.Value ?? "empty";
});

app.MapGet("/jwt", () =>
{
    // this uses the RSA private key to sign the JWT token
    var key = new RsaSecurityKey(rsaKey); // this is the public and private key pair
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

app.MapGet("/jwk", () => // this returns a JWKS (with one key) because we don't want to use OIDC to do it
{
    var publicKey = RSA.Create();
    publicKey.ImportRSAPublicKey(rsaKey.ExportRSAPublicKey(), out _);
    var key = new RsaSecurityKey(publicKey); // this is just the public key
    var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
    return jwk;
});

app.Run();
