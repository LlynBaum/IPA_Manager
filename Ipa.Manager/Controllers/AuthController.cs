using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ipa.Manager.Controllers;

[Authorize]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<IActionResult> LoginAsync([FromForm] LoginRequest loginRequest)
    {
        // TODO: get user from db
        var username = "";
        var userId = 1;
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
            });

        return Redirect("/");
    }

    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterRequest registerRequest)
    {
        // TODO: create user
        return await LoginAsync(new LoginRequest(registerRequest.Username, registerRequest.Password));
    }
    
    [HttpGet("/logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }

    public record LoginRequest(string Username, string Password);

    public record RegisterRequest(string Username, string Password);
}