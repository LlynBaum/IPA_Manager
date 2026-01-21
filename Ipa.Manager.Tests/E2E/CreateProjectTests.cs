using Ipa.Manager.Models;
using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class CreateProjectTests : PlaywrightTestBase
{
    private const string Username = "TestUser";
    private const string Password = "TestPassword123!";

    protected override bool EnableTracing => true;

    [Test]
    public async Task CreateProject_CreatesProjectInDb()
    {
        await CreateUserAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + "create-project");

        // Fill in the form
        await Page.GetByPlaceholder("Enter IPA name").FillAsync("My Awesome IPA");
        await Page.GetByPlaceholder("Enter topic").FillAsync("Web Development with Blazor");

        // Select Criteria (A01 and A02)
        // We can locate them by the ID badge text or the criteria name.
        // Assuming A01 and A02 are present in the seeded data.
        await Page.Locator(".criteria-item").Filter(new() { HasText = "A01" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.Locator(".criteria-item").Filter(new() { HasText = "A02" }).GetByRole(AriaRole.Checkbox).CheckAsync();

        // Submit
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Project" }).ClickAsync();

        // Expect redirection to Home
        await Expect(Page).ToHaveURLAsync(BaseUrl);

        // Verify DB
        var project = Db.Projects.SingleOrDefault();
        Assert.That(project, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(project.Name, Is.EqualTo("My Awesome IPA"));
            Assert.That(project.Topic, Is.EqualTo("Web Development with Blazor"));
            Assert.That(project.UserId, Is.Not.Zero);
        });

        // Verify Criteria Progress
        var criteriaProgress = Db.CriteriaProgress.Where(cp => cp.ProjectId == project.Id).ToList();
        Assert.That(criteriaProgress, Has.Count.EqualTo(2));
        Assert.That(criteriaProgress.Select(cp => cp.CriteriaId), Is.EquivalentTo(new[] { "A01", "A02" }));
    }

    [Test]
    public async Task CreateProject_ShowsValidationErrors_WhenFieldsAreEmpty()
    {
        await CreateUserAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl + "create-project");

        // Submit without filling anything
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Project" }).ClickAsync();

        // Expect validation messages
        await Expect(Page.GetByText("The Name field is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("The Topic field is required.")).ToBeVisibleAsync();

        // Verify no project created
        Assert.That(Db.Projects.Count(), Is.EqualTo(0));
    }

    private async Task CreateUserAndLoginAsync()
    {
        // Create user in DB directly
        var passwordHasher = ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var user = new User
        {
            Username = Username,
            PasswordHash = passwordHasher.HashPassword(null!, Password)
        };
        await Db.Users.AddAsync(user);
        await Db.SaveChangesAsync();

        // Login via API (bypass UI)
        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = Username,
            ["Password"] = Password,
            ["ReturnUrl"] = "/"
        });
        var formData = await formContent.ReadAsStringAsync();

        var response = await Context.APIRequest.PostAsync(BaseUrl + "auth/login", new()
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" },
            Data = formData
        });

        Assert.That(response.Status, Is.EqualTo(200).Or.EqualTo(302)); // Redirect or OK
    }
}
