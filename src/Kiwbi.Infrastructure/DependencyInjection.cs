using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Domain.Developers;
using Kiwbi.Infrastructure.Identity;
using Kiwbi.Infrastructure.Persistence;
using Kiwbi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kiwbi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<KiwbiDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<KiwbiDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();

        services.AddScoped<IDeveloperCompanyRepository, DeveloperCompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAccountProvisioningService, IdentityAccountProvisioningService>();
        services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();

        return services;
    }
}
