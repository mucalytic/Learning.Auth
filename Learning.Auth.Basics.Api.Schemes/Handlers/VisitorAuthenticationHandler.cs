using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace Learning.Auth.Basics.Api.Schemes.Handlers;

public class VisitorAuthenticationHandler(
    IOptionsMonitor<CookieAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : CookieAuthenticationHandler(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync();
        if (result.Succeeded) return result;
        IEnumerable<Claim> claims = [new("usr", "aaron")];
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);
        await Context.SignInAsync("visitor", user);
        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
