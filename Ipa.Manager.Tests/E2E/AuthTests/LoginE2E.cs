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

        // TODO: fill in forms
        await Page.GetByRole(AriaRole.Button).ClickAsync();
        
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