//using Ipa.Manager.Database;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
[NonParallelizable]
public class PlaywrightTestBase : PageTest
{
    protected string BaseUrl = string.Empty;

    protected IServiceScope Scope;
    //protected ApplicationDbContext Db;
    
    [SetUp]
    public Task SetUp()
    {
        BaseUrl = PlaywrightServerFixture.Factory.ServerAddress;
        
        Scope = PlaywrightServerFixture.Factory.Services.CreateScope();
        //Db = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //await Db.Database.EnsureDeletedAsync();
        //await Db.Database.MigrateAsync();
        return Task.CompletedTask;
    }

    [TearDown]
    public void TearDown()
    {
        //Db.Dispose();
        Scope.Dispose();
    }
}