using Learning.Auth.OAuth.Custom.Server.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text;

namespace Learning.Auth.OAuth.Custom.Server.Endpoints.OAuth;

public static class PostToken
{
    public static async Task<IResult> Handler(
        HttpRequest request,
        DevKeysService devKeysService,
        ValidationCodeVerifier verifier,
        IDataProtectionProvider provider)
    {
        var bodyBytes = await request.BodyReader.ReadAsync();
        var bodyContent = Encoding.UTF8.GetString(bodyBytes.Buffer);
        string grantType = "", codeString = "", redirectUri = "", codeVerifier = "";
        foreach (var part in bodyContent.Split('&'))
        {
            var keyValue = part.Split('=');
            if (keyValue[0] == "grant_type") grantType = keyValue[1];
            if (keyValue[0] == "code") codeString = keyValue[1];
            if (keyValue[0] == "redirect_uri") redirectUri = keyValue[1];
            if (keyValue[0] == "code_verifier") codeVerifier = keyValue[1];
        }
        var protector = provider.CreateProtector("oauth");
        var code = protector.Unprotect(codeString);
        var authCode = JsonSerializer.Deserialize<AuthCode>(code);
        if (authCode is null || !verifier.ValidateS256(codeVerifier, authCode)) return Results.BadRequest();
        var handler = new JsonWebTokenHandler();
        return Results.Ok(new
        {
            access_token = handler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Claims = new Dictionary<string, object>
                    {
                        [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString(),
                        ["custom"] = "foo"
                    },
                    Expires = DateTime.Now.AddMinutes(15),
                    TokenType = "Bearer",
                    SigningCredentials = new SigningCredentials(devKeysService.RsaSecurityKey, SecurityAlgorithms.RsaSha256)
                }),
            token_type = "Bearer"
        });
    }
}
