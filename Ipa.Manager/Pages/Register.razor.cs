using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Register(NavigationManager navigationManager) : ComponentBase
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }
    // TODO: prevent submitting when passwords do not match
    private void ToLogin()
    {
        navigationManager.NavigateTo("/login");
    }
}