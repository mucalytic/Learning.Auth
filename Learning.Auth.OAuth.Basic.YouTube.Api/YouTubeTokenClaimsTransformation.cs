using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Learning.Auth.OAuth.Basic.YouTube.Api;

public class YouTubeTokenClaimsTransformation(Database db) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue("user_id");
        if (userId is null || !db.TryGetValue(userId, out var token)) return Task.FromResult(principal);
        var user = principal.Clone();
        var identity = principal.Identities.FirstOrDefault(id => id.AuthenticationType == "cookie");
        if (identity is null) return Task.FromResult(principal);
        identity.AddClaim(new Claim("youtube-access-token", token));
        return Task.FromResult(user);
    }
}
