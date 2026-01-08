using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Login(NavigationManager navigationManager) : ComponentBase
{
    [SupplyParameterFromQuery(Name = "ReturnUrl")]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "LoginFailed")] 
    public bool LoginFailed { get; set; } = false;

    private string? username;
    private string? password;

    protected override Task OnParametersSetAsync()
    {
        _ = navigationManager.Uri;
        return base.OnParametersSetAsync();
    }

    private bool IsInputValid()
    {
        return username is not null 
               && username.Length > 0 
               && password is not null 
               && password.Length > 0;
    }
    
    private void ToRegister()
    {
        var registerUri = "/register" + QueryString.Create("ReturnUrl", ReturnUrl ?? "/");
        navigationManager.NavigateTo(registerUri);
    }
}