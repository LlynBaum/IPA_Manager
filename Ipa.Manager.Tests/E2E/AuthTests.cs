using Ipa.Manager.Models;
using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class AuthTests : PlaywrightTestBase
{
    private const string Username = "Test";
    private const string Password = "abcdefg";
    
    protected override bool EnableTracing => true;

    private ILocator UsernameInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "username" });
    private ILocator PasswordInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "password" });

    [Test]
    public async Task UnauthenticatedUser_IsRedirectedToLogin()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}login?ReturnUrl=%2F"));
    }
    
    [Test]
    public async Task Register_CreatesUserInDb()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");


        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync(Password);
        await PasswordInput.Last.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
        
        var user = Db.Users.SingleOrDefault();
        Assert.That(user, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(user.Username, Is.EqualTo(Username));
            Assert.That(user.PasswordHash, Is.Not.EqualTo(Password));
        });
    }

    [Test]
    public async Task Register_CreateAccountDisabled_WhenPasswordConfirmationDoesNotMatch()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");

        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync(Password);
        await PasswordInput.Last.InteractiveFillAsync(Password + "Bla");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).IsDisabledAsync();
    }
    
    [Test]
    public async Task Register_RedirectsToReturnUrl_OnSuccess()
    {
        const string returnUrl = "https://www.google.com";
        var loginUri = BaseUrl + "login" + QueryString.Create("ReturnUrl", returnUrl);
        await Page.GotoSaveAsync(loginUri);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");

        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync(Password);
        await PasswordInput.Last.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo(returnUrl + "/"));
    }
    
    [Test]
    public async Task Register_ShowsError_WhenUsernameIsAlreadyInUse()
    {
        await CreateTestUserAsync();
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");

        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync(Password);
        await PasswordInput.Last.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await Expect(Page.GetByText("Username is already in use.")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Register_ShowsError_WhenUsernameIsTooLong()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");

        var username = string.Join(null, Enumerable.Repeat('a', 105));
        
        await UsernameInput.InteractiveFillAsync(username);
        await PasswordInput.First.InteractiveFillAsync(Password);
        await PasswordInput.Last.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await Expect(Page.GetByText("Username is too long.")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Register_ShowsError_WhenPasswordIsTooLong()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");

        var password = string.Join(null, Enumerable.Repeat('a', 35));
        
        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync(password);
        await PasswordInput.Last.InteractiveFillAsync(password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await Expect(Page.GetByText("Password is too long.")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Register_ShowsError_WhenPasswordIsTooShort()
    {
        await Page.GotoSaveAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Create Account");
        
        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.First.InteractiveFillAsync("ab");
        await PasswordInput.Last.InteractiveFillAsync("ab");
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await Expect(Page.GetByText("Password is too short.")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Login_LogisInTheUser_OnSuccess()
    {
        await CreateTestUserAsync();
        await Page.GotoSaveAsync(BaseUrl);

        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo(BaseUrl));
    }
    
    [Test]
    public async Task Login_GoesBackToLogin_WhenPasswordIsWrong()
    {
        await CreateTestUserAsync();
        await Page.GotoSaveAsync(BaseUrl);

        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.InteractiveFillAsync("wrong-password");
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        Assert.That(Page.Url, Does.StartWith($"{BaseUrl}login"));
        await Expect(Page.GetByText("Wrong Password")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Login_GoesBackToLogin_WhenUserDoesNotExist()
    {
        await CreateTestUserAsync();
        await Page.GotoSaveAsync(BaseUrl);

        await UsernameInput.InteractiveFillAsync("Unknown User");
        await PasswordInput.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        Assert.That(Page.Url, Does.StartWith($"{BaseUrl}login"));
        await Expect(Page.GetByText("User does not Exist")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Login_RedirectsToReturnUrl_OnSuccess()
    {
        await CreateTestUserAsync();
        
        const string returnUrl = "https://www.google.com";
        var loginUri = BaseUrl + "login" + QueryString.Create("ReturnUrl", returnUrl);
        await Page.GotoSaveAsync(loginUri);
        
        await UsernameInput.InteractiveFillAsync(Username);
        await PasswordInput.InteractiveFillAsync(Password);
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo(returnUrl + "/"));
    }

    private async Task CreateTestUserAsync()
    {
        var passwordHasher = ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var user = new User
        {
            Username = Username,
            PasswordHash = passwordHasher.HashPassword(null!, Password)
        };
        await Db.Users.AddAsync(user);
        await Db.SaveChangesAsync();
    }
}