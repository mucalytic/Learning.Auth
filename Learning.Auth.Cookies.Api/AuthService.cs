using Microsoft.AspNetCore.DataProtection;

namespace Learning.Auth.Cookies.Api;

public class AuthService(IDataProtectionProvider provider, IHttpContextAccessor accessor)
{
    public void SignIn()
    {
        var protector = provider.CreateProtector("auth-cookie");
        var cookie = $"auth={protector.Protect("usr:aaron")}";
        if (accessor.HttpContext is null) return;
        accessor.HttpContext.Response.Headers["set-cookie"] = cookie;
    }
}
