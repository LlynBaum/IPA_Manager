using System.Security.Claims;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Auth;

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
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", loginRequest.ReturnUrl ?? "/" },
                { "LoginFailed", "true" }
            };
            var loginUri = "/login" + QueryString.Create(queryString);
            return Redirect(loginUri);
        }

        var loginResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
        if (loginResult != PasswordVerificationResult.Success)
        {
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", loginRequest.ReturnUrl ?? "/" },
                { "LoginFailed", "true" }
            };
            var loginUri = "/login" + QueryString.Create(queryString);
            return Redirect(loginUri);
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

        return Redirect(loginRequest.ReturnUrl ?? "/");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        if (registerRequest.Username.Length > 100)
        {   
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", registerRequest.ReturnUrl ?? "/" },
                { "RegisterError", "Username too long" }
            };
            var loginUri = "/register" + QueryString.Create(queryString);
            return Redirect(loginUri);
        }
        
        if (registerRequest.Password.Length > 30)
        {   
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", registerRequest.ReturnUrl ?? "/" },
                { "RegisterError", "Password is too long." }
            };
            var loginUri = "/register" + QueryString.Create(queryString);
            return Redirect(loginUri);
        }
        
        if (registerRequest.Password.Length < 5)
        {   
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", registerRequest.ReturnUrl ?? "/" },
                { "RegisterError", "Password is too short." }
            };
            var loginUri = "/register" + QueryString.Create(queryString);
            return Redirect(loginUri);
        }

        if (await context.Users.AnyAsync(u => u.Username == registerRequest.Username, cancellationToken))
        {
            var queryString = new Dictionary<string, string?>
            {
                { "ReturnUrl", registerRequest.ReturnUrl ?? "/" },
                { "RegisterError", "Username already in use." }
            };
            var loginUri = "/register" + QueryString.Create(queryString);
            return Redirect(loginUri);
        }
        
        // The default Implementation of PasswordHasher does not use the value of user
        var passwordHash = passwordHasher.HashPassword(null!, registerRequest.Password);
        var user = new User
        {
            Username = registerRequest.Username,
            PasswordHash = passwordHash
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return await LoginAsync(new LoginRequest(registerRequest.Username, registerRequest.Password, registerRequest.ReturnUrl), cancellationToken);
    }
    
    [HttpGet("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }

    public record LoginRequest(string Username, string Password, string? ReturnUrl);

    public record RegisterRequest(string Username, string Password, string? ReturnUrl);
}