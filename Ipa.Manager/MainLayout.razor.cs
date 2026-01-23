using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Ipa.Manager;

public partial class MainLayout : IDisposable
{
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    private bool ShowScrollToTop => !string.IsNullOrEmpty(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }

    private async Task ScrollToTop()
    {
        await JSRuntime.InvokeVoidAsync("window.scrollTo", new { top = 0, behavior = "smooth" });
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }
}