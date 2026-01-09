using Ipa.Manager.Models;
using Ipa.Manager.Services.Criterias;
using Microsoft.AspNetCore.Identity;

namespace Ipa.Manager.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddIpaServices(this IServiceCollection service)
    {
        service.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        service.AddSingleton<ICriteriaService, CriteriaService>();
        return service;
    }
}