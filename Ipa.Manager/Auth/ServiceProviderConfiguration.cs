using Microsoft.AspNetCore.Authentication.Cookies;

namespace Ipa.Manager.Auth;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddCookieAuth(this IServiceCollection service)
    {
        service.AddAuthorization();
        service.AddCascadingAuthenticationState();
        service.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "wih.auth";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/denied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

        return service;
    }
}