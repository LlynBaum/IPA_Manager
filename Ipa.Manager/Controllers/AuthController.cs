using System.Security.Claims;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Controllers;

[Authorize]
[Route("auth")]
public class AuthController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromForm] LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Username == loginRequest.Username, cancellationToken);

        if (user is null)
        {
            return Redirect("/login");
        }

        var loginResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
        if (loginResult != PasswordVerificationResult.Success)
        {
            return Redirect("/login");
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
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
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        // The default Implementation of PasswordHasher does not use the value of user
        var passwordHash = passwordHasher.HashPassword(null!, registerRequest.Password);
        var user = new User
        {
            Username = registerRequest.Username,
            PasswordHash = passwordHash
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return await LoginAsync(new LoginRequest(registerRequest.Username, registerRequest.Password), cancellationToken);
    }
    
    [HttpGet("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }

    public record LoginRequest(string Username, string Password);

    public record RegisterRequest(string Username, string Password);
}