using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learning.Auth.Cookies.Api.Controllers;

public class HomeController : Controller
{
    [HttpPost("/mvc/login")]
    public async Task<IActionResult> Login()
    {
        IEnumerable<Claim> claims = [
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) // userId
        ];
        var identity = new ClaimsIdentity(claims, "default"); // authentication type can be anything
        var user = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync("default", user); // must match the registered authentication (cookie) scheme
        return Ok();
    }
}
