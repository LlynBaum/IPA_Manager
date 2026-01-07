using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class E2EExample : PlaywrightTestBase
{
    [Test]
    public async Task Ka()
    {
        await Page.GotoAsync(BaseUrl);
        // Db.Users.Add(new User());
        // await Db.SaveChangesAsync();
    }
}