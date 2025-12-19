using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Ipa.Manager.Tests.AuthTests;

public class LoginE2E : PageTest
{
    [Test]
    public async Task Ka()
    {
        await Page.GotoAsync("http://localhost:5048");

        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo("http://localhost:5048/login?ReturnUrl=%2F"));
        
        
    }
}