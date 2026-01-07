using Ipa.Manager.Tests.E2E.Framework;
using Ipa.Manager.Models;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class E2EExample : PlaywrightTestBase
{
    [Test]
    public async Task Ka()
    {
        // Use BaseUrl
        await Page.GotoAsync(BaseUrl); 
        
        // Access DI
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Use DB

        var user = new User
        {
            Username = "Test",
            PasswordHash = "abcdefg"
        };
        
        await Db.Users.AddAsync(user); 
        await Db.SaveChangesAsync();
    }
}