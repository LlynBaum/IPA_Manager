using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Login(NavigationManager navigationManager) : ComponentBase
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    private void ToRegister()
    {
        navigationManager.NavigateTo("/register");
    }
}