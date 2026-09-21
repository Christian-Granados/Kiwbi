using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Application.Onboarding;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Identity;
using Kiwbi.Infrastructure.Email;
using Kiwbi.Infrastructure.Onboarding;
using Kiwbi.Infrastructure.Persistence;
using Kiwbi.Infrastructure.Reporting;
using Kiwbi.Infrastructure.Repositories;
using Kiwbi.Infrastructure.Storage;
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

        // QuestPDF requires the license to be set once per process; Community is free below the revenue threshold this project qualifies for.
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<KiwbiDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();

        services.AddScoped<IDeveloperCompanyRepository, DeveloperCompanyRepository>();
        services.AddScoped<IHousingPromotionRepository, HousingPromotionRepository>();
        services.AddScoped<IHousingTypologyRepository, HousingTypologyRepository>();
        services.AddScoped<IHousingUnitRepository, HousingUnitRepository>();
        services.AddScoped<ITradeCategoryRepository, TradeCategoryRepository>();
        services.AddScoped<ICustomizationRepository, CustomizationRepository>();
        services.AddScoped<IBuyerInvitationRepository, BuyerInvitationRepository>();
        services.AddScoped<IHousingUnitBuyerRepository, HousingUnitBuyerRepository>();
        services.AddScoped<IHomeCustomizationChoiceRepository, HomeCustomizationChoiceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAccountProvisioningService, IdentityAccountProvisioningService>();
        services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();

        // Epic 11, Feature 11.4/11.5: pick the cloud adapter when configured (Storage:Provider=S3 /
        // Email:Provider=Smtp - both set as Render environment variables, never committed), otherwise fall back
        // to the local-disk/logging adapters used by `dotnet run`/`dotnet watch` in Development.
        if (string.Equals(configuration["Storage:Provider"], "S3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, S3FileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        if (string.Equals(configuration["Email:Provider"], "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, LoggingBuyerInvitationEmailSender>();
        }

        services.AddScoped<IBuyerAccountProvisioningService, IdentityBuyerAccountProvisioningService>();
        services.AddScoped<IHousingPromotionReportGenerator, HousingPromotionReportGenerator>();

        return services;
    }
}
