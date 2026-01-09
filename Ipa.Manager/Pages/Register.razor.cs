using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Register(NavigationManager navigationManager) : ComponentBase
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }
    
    [SupplyParameterFromQuery]
    public string? RegisterError { get; set; }
    
    private bool disableForms;
    private string? username;
    private string? password;
    private string? passwordConfirmation;
    
    protected override void OnAfterRender(bool firstRender)
    {
        disableForms = true;
    }
    
    private bool IsInputValid()
    {
        if (string.IsNullOrEmpty(username)) return false;
        if (string.IsNullOrEmpty(password)) return false;
        if (string.IsNullOrEmpty(passwordConfirmation)) return false;

        return password == passwordConfirmation;
    }
    
    private void ToLogin()
    {
        var loginUri = "/login" + QueryString.Create("ReturnUrl", ReturnUrl ?? "/");
        navigationManager.NavigateTo(loginUri);
    }
}