//using Ipa.Manager.Database;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[NonParallelizable]
public class PlaywrightTestBase(bool enableTracing = false) : PageTest
{
    /// <summary>
    /// The URL where the Blazor host is available.
    /// </summary>
    protected string BaseUrl = string.Empty;
    
    /// <summary>
    /// A DB Context for the current DB Instance. The data in the DB will be cleared after every Test run.
    /// </summary>
    //protected ApplicationDbContext Db;

    /// <summary>
    /// The ServiceProvider to access the DI Container.
    /// </summary>
    protected IServiceProvider ServiceProvider => scope.ServiceProvider;
    
    private IServiceScope scope;
    
    [SetUp]
    public async Task Setup()
    {
        if(!enableTracing) return;
        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }
    
    [TearDown]
    public async Task TearDown()
    {
        if(!enableTracing) return;
        await Context.Tracing.StopAsync(new()
        {
            Path = "trace.zip"
        });
    }
    
    [SetUp]
    public async Task StartServer()
    {
        BaseUrl = PlaywrightServerFixture.Factory.ServerAddress;
        
        scope = PlaywrightServerFixture.Factory.Services.CreateScope();
        //Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //await Db.Database.EnsureDeletedAsync();
        //await Db.Database.MigrateAsync();
    }

    [TearDown]
    public void DisposeScope()
    {
        //Db.Dispose();
        scope.Dispose();
    }
}