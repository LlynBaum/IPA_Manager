using Ipa.Manager.Services.Criterias;

namespace Ipa.Manager.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddIpaServices(this IServiceCollection service)
    {
        service.AddSingleton<ICriteriaService, CriteriaService>();
        return service;
    }
}