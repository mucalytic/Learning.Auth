using Learning.Auth.Basics.Api.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Learning.Auth.Basics.Api.Services;

public class AuthenticationService(IDataProtectionProvider idp, IHttpContextAccessor hca) : IAuthenticationService
{
    public void SignIn()
    {
        var protector = idp.CreateProtector("auth-cookie");
        hca.HttpContext?.Response.Headers.SetCookie = $"auth={protector.Protect("usr:aaron")}";
    }
}
