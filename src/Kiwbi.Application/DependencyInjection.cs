using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.Developers.Login;
using Kiwbi.Application.Developers.Logout;
using Kiwbi.Application.Developers.RegisterDeveloper;
using Kiwbi.Application.Developers.UpdateDeveloperBranding;
using Kiwbi.Application.Developers.UpdateDeveloperProfile;
using Microsoft.Extensions.DependencyInjection;

namespace Kiwbi.Application;

public static class DependencyInjection
{
    /// <summary>Registers application-layer use cases and services. Extend as use cases are added.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<RegisterDeveloperUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<LogoutUseCase>();
        services.AddScoped<GetCurrentDeveloperProfileUseCase>();
        services.AddScoped<UpdateDeveloperProfileUseCase>();
        services.AddScoped<UpdateDeveloperBrandingUseCase>();

        return services;
    }
}
