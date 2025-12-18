using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Login : ComponentBase
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }
}