using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Web;

namespace Learning.Auth.OAuth.Custom.Server.Endpoints.OAuth;

public static class GetAuthorize
{
    public static IResult Handler(HttpRequest request, IDataProtectionProvider provider)
    {
        request.Query.TryGetValue("scope", out var scope);
        request.Query.TryGetValue("state", out var state);
        request.Query.TryGetValue("client_id", out var clientId);
        request.Query.TryGetValue("redirect_uri", out var redirectUri);
        request.Query.TryGetValue("response_type", out var responseType);
        request.Query.TryGetValue("code_challenge", out var codeChallenge);
        request.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod);
        var issuer = HttpUtility.UrlEncode("https://localhost:7045");
        var protector = provider.CreateProtector("oauth");
        var authCode = new AuthCode(
            clientId.ToString(),
            codeChallenge.ToString(),
            codeChallengeMethod.ToString(),
            redirectUri.ToString(),
            DateTime.Now.AddMinutes(5));
        var code = protector.Protect(JsonSerializer.Serialize(authCode));
        return Results.Redirect($"{redirectUri}?code={code}&state={state}&iss={issuer}");
    }
}
