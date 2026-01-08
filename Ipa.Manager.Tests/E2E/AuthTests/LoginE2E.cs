using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E.AuthTests;

[TestFixture]
public class LoginE2E : PlaywrightTestBase
{
    private const string Username = "Test";
    private const string Password = "abcdefg";
    
    protected override bool EnableTracing => true;

    private ILocator UsernameInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "username" });
    private ILocator PasswordInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "password" });

    [Test]
    public async Task UnauthenticatedUser_IsRedirectedToLogin()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}login?ReturnUrl=%2F"));
    }
    
    [Test]
    public async Task Register_CreatesUserInDb()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

        await UsernameInput.FillAsync(Username);
        await PasswordInput.First.FillAsync(Password);
        await PasswordInput.Last.FillAsync(Password);
        
        await PasswordInput.Last.BlurAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo(BaseUrl)); 
        
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
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

        await UsernameInput.FillAsync(Username);
        await PasswordInput.First.FillAsync(Password);
        await PasswordInput.Last.FillAsync(Password + "Bla");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).IsDisabledAsync();
    }
    
    [Test]
    public async Task Register_RedirectsToReturnUrl_OnSuccess()
    {
        const string returnUrl = "https://www.google.com";
        var loginUri = "/" + QueryString.Create("ReturnUrl", returnUrl);
        await Page.GotoAsync(loginUri);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

        await UsernameInput.FillAsync(Username);
        await PasswordInput.First.FillAsync(Password);
        await PasswordInput.Last.FillAsync(Password);
        
        await PasswordInput.Last.BlurAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo("www.google.com"));
    }
    
    [Test]
    public async Task Login_LogisInTheUser_OnSuccess()
    {
        
    }
    
    [Test]
    public async Task Login_GoesBackToLogin_WhenLoginFails()
    {
        
    }
    
    [Test]
    public async Task Login_RedirectsToReturnUrl_OnSuccess()
    {
        
    }
}