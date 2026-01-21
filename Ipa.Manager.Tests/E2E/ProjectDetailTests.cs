using Ipa.Manager.Models;
using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class ProjectDetailTests : PlaywrightTestBase
{
    private const string Username = "DetailUser";
    private const string Password = "TestPassword123!";

    protected override bool EnableTracing => true;

    [Test]
    public async Task ProjectDetail_ShowsCorrectInfo()
    {
        var (_, project) = await CreateProjectAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + $"project/{project.Id}");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Detail Test Project" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Testing Details")).ToBeVisibleAsync();
        await Expect(Page.GetByText("A01")).ToBeVisibleAsync();
        await Expect(Page.Locator("textarea")).ToHaveValueAsync("Initial Note");
    }

    [Test]
    public async Task ProjectDetail_CanUpdateCriteria()
    {
        var (_, project) = await CreateProjectAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + $"project/{project.Id}");

        // Check the first checkbox for A01
        var checkbox = Page.Locator(".quality-level-item input[type='checkbox']").First;
        await checkbox.CheckAsync();

        // Reload to verify persistence
        await Page.ReloadAsync();
        await Expect(checkbox).ToBeCheckedAsync();

        // Verify in DB
        Db.ChangeTracker.Clear();
        var progress = Db.CriteriaProgress.Single(cp => cp.ProjectId == project.Id && cp.CriteriaId == "A01");
        // We clicked the first checkbox, which now corresponds to the highest index (3) due to reversed loop
        Assert.That(progress.FulfilledRequirementIds, Contains.Item(3));
    }

    [Test]
    public async Task ProjectDetail_CanUpdateNotes()
    {
        var (_, project) = await CreateProjectAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + $"project/{project.Id}");

        var notesArea = Page.Locator("textarea");
        await notesArea.FillAsync("Updated Note Content");
        
        // Trigger change event (blur)
        await notesArea.BlurAsync();

        // Reload
        await Page.ReloadAsync();
        await Expect(notesArea).ToHaveValueAsync("Updated Note Content");
    }

    [Test]
    public async Task ProjectDetail_CanEditProject()
    {
        var (_, project) = await CreateProjectAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + $"project/{project.Id}");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        
        await Page.Locator("input[type='text']").First.FillAsync("Renamed Project");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Renamed Project" })).ToBeVisibleAsync();
        
        // Verify DB
        Db.ChangeTracker.Clear();
        var updatedProject = Db.Projects.Find(project.Id);
        Assert.That(updatedProject!.Name, Is.EqualTo("Renamed Project"));
    }

    [Test]
    public async Task ProjectDetail_CanDeleteProject()
    {
        var (_, project) = await CreateProjectAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + $"project/{project.Id}");

        // Handle dialog
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        // Should redirect to home
        await Expect(Page).ToHaveURLAsync(BaseUrl);

        // Verify DB
        Db.ChangeTracker.Clear();
        var deletedProject = Db.Projects.Find(project.Id);
        Assert.That(deletedProject, Is.Null);
    }

    private async Task<(User User, Project Project)> CreateProjectAndLoginAsync()
    {
        // Create user
        var passwordHasher = ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var user = new User
        {
            Username = Username,
            PasswordHash = passwordHasher.HashPassword(null!, Password)
        };
        await Db.Users.AddAsync(user);
        await Db.SaveChangesAsync();

        // Create project
        var project = new Project
        {
            Name = "Detail Test Project",
            Topic = "Testing Details",
            UserId = user.Id
        };
        await Db.Projects.AddAsync(project);

        // Add criteria
        await Db.CriteriaProgress.AddAsync(new CriteriaProgress
        {
            Project = project,
            CriteriaId = "A01",
            FulfilledRequirementIds = [],
            Notes = "Initial Note"
        });
        await Db.SaveChangesAsync();

        // Login
        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = Username,
            ["Password"] = Password,
            ["ReturnUrl"] = "/"
        });
        var formData = await formContent.ReadAsStringAsync();

        await Context.APIRequest.PostAsync(BaseUrl + "auth/login", new()
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" },
            Data = formData
        });

        return (user, project);
    }
}