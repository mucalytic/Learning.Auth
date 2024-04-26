using System.Security.Claims;

namespace Learning.Auth.Identity.Cookies.Api;

public static class UserExtensions
{
    public static ClaimsPrincipal ToClaimsPrincipal(this User user, string authenticationScheme) =>
        new(new ClaimsIdentity(user.Claims.Select(c => new Claim(c.Key, c.Value)), authenticationScheme));
}
