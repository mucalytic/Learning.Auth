using static Microsoft.IdentityModel.Tokens.SecurityAlgorithms;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Learning.Auth.Jwt.Revocation.Blacklists;
using Learning.Auth.Jwt.Revocation.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

var rsaKey = RSA.Create();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();
builder.Services.AddAuthentication("jwt").AddJwtBearer("jwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = async context => // token can come from the query (as an example)
        {
            if (context.Request.Query.ContainsKey("t"))
            {
                context.Token = context.Request.Query["t"];
                if (context.Token is null) return;
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();
                var hash = Encoding.UTF8.GetBytes(context.Token);
                var bs64 = Convert.ToBase64String(hash);
                var blacklisted = await blacklist.IsBlacklistedAsync(bs64);
                if (blacklisted) context.Fail("Token has been invalidated");
            }
        }
    };
    options.Configuration = new OpenIdConnectConfiguration
    {
        SigningKeys =
        {
            new RsaSecurityKey(rsaKey)
        }
    };
    options.MapInboundClaims = false;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/login", () =>
{
    var key = new RsaSecurityKey(rsaKey);
    var handler = new JsonWebTokenHandler();
    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = "https://localhost:7000",
        Subject =  new ClaimsIdentity([
            new Claim("sub", Guid.NewGuid().ToString())
        ]),
        SigningCredentials = new SigningCredentials(key, RsaSha256)
    };
    return handler.CreateToken(descriptor);
});
app.MapGet("/user", (ClaimsPrincipal user) =>
    user.Claims.Select(claim => new { claim.Type, claim.Value }).ToList());
app.MapGet("/blacklist", async (ITokenBlacklist blacklist, string token) =>
{
    // token can be quite big, so let's hash it first
    var hash = Encoding.UTF8.GetBytes(token);
    var bs64 = Convert.ToBase64String(hash);
    await blacklist.BlacklistAsync(bs64, DateTime.UtcNow.AddMinutes(10));
});
app.Run();
