using Microsoft.Extensions.DependencyInjection;

namespace Kiwbi.Application;

public static class DependencyInjection
{
    /// <summary>Registers application-layer use cases and services. Extend as use cases are added.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
