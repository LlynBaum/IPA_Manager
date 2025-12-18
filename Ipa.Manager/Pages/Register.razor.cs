using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ipa.Manager.Pages;

[AllowAnonymous]
public partial class Register : ComponentBase
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }
}