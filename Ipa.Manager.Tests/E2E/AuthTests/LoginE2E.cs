using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E.AuthTests;

public class LoginE2E : PlaywrightTestBase
{
    [Test]
    public async Task Ka()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
        Assert.That(Page.Url, Is.EqualTo($"{BaseUrl}login?ReturnUrl=%2F"));
    }
}