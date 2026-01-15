using Microsoft.Playwright;

namespace Ipa.Manager.Tests.E2E.Framework;

public static class PlaywrightUtils
{
    public static async Task GotoSaveAsync(this IPage page, string url)
    {
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
    
    public static async Task InteractiveFillAsync(this ILocator locator, string value)
    {
        await locator.FillAsync(value);
        await Assertions.Expect(locator).ToHaveValueAsync(value);
    }
}