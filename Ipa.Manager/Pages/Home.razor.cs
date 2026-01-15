using Ipa.Manager.Auth;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Pages;

[Authorize]
public partial class Home(
    ApplicationDbContext context, 
    UserContext userContext,
    NavigationManager navigationManager)
{
    private IReadOnlyList<Project> projects = [];
    
    protected override async Task OnInitializedAsync()
    {
        projects = await context.Projects
            .Where(p => p.UserId == userContext.UserId)
            .ToListAsync();
    }

    private void ViewProject(int id)
    {
        navigationManager.NavigateTo($"/project/{id}");
    }
}