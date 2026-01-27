using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

// by hard-coding the JWK here, we circumvent the need to go to the auth server to get the public key
var jwkString = "";

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
                JsonWebKey.Create(jwkString) // ← this is the public key. it can only be used to verify a signature.
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

app.Run();
