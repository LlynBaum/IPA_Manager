using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E.AuthTests;

[TestFixture]
public class LoginE2E() : PlaywrightTestBase(true)
{
    private ILocator UsernameInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "username" });
    private ILocator PasswordInput => Page.GetByRole(AriaRole.Textbox, new() { Name = "password" });

    [Test]
    public async Task UnauthorizedUser_IsRedirectedToLogin()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}login?ReturnUrl=%2F"));
    }
    
    [Test]
    public async Task Register_CreatesUserInDb()
    {
        const string username = "Test";
        const string password = "abcdefg";
        
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

        await UsernameInput.FillAsync(username);
        await PasswordInput.First.FillAsync(password);
        await PasswordInput.Last.FillAsync(password);
        
        await PasswordInput.Last.BlurAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
        
        Assert.That(Page.Url, Is.EqualTo(BaseUrl)); 
        
        var user = Db.Users.SingleOrDefault();
        Assert.That(user, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(user.Username, Is.EqualTo(username));
            Assert.That(user.PasswordHash, Is.Not.EqualTo(password));
        });
    }

    [Test]
    public async Task Register_RedirectsBackAndDoesNotCreateUser_WhenRegisterFails()
    {
        
    }
    
    [Test]
    public async Task Register_RedirectsToReturnUrl_OnSuccess()
    {
        
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