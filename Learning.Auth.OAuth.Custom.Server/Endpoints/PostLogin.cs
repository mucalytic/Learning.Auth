using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Learning.Auth.OAuth.Custom.Server.Endpoints;

public static class PostLogin
{
    public static async Task<IResult> Handler(HttpContext context, string returnUrl)
    {
        await context.SignInAsync("cookie",
            new ClaimsPrincipal(
                new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                ],"cookie")));
        return Results.Redirect(returnUrl);
    }
}
