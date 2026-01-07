using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E.AuthTests;

public class LoginE2E : PlaywrightTestBase
{
    [Test]
    public async Task RegisterCreatesUserInDb()
    {
        const string username = "Test";
        const string password = "abcdefg";
        
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}login?ReturnUrl=%2F"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Task.Delay(100);
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}register?ReturnUrl=%2F"));

        await Page.GetByRole(AriaRole.Textbox, new () { Name = "username" }).FillAsync(username);
        await Page.GetByRole(AriaRole.Textbox, new () { Name = "password" }).First.FillAsync(password);
        await Page.GetByRole(AriaRole.Textbox, new () { Name = "password" }).Last.FillAsync(password);
        
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
}