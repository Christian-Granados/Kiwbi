using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.Developers.Login;
using Kiwbi.Application.Developers.Logout;
using Kiwbi.Application.Developers.RegisterDeveloper;
using Kiwbi.Application.Developers.UpdateDeveloperBranding;
using Kiwbi.Application.Developers.UpdateDeveloperProfile;
using Kiwbi.Application.RealEstate.ChangeHousingUnitStatus;
using Kiwbi.Application.RealEstate.CreateHousingPromotion;
using Kiwbi.Application.RealEstate.CreateHousingTypology;
using Kiwbi.Application.RealEstate.CreateHousingUnit;
using Kiwbi.Application.RealEstate.DeleteHousingPromotion;
using Kiwbi.Application.RealEstate.DeleteHousingTypology;
using Kiwbi.Application.RealEstate.DeleteHousingUnit;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingPromotions;
using Kiwbi.Application.RealEstate.GetHousingPromotionSummary;
using Kiwbi.Application.RealEstate.GetHousingTypologies;
using Kiwbi.Application.RealEstate.GetHousingTypology;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Application.RealEstate.GetHousingUnits;
using Kiwbi.Application.RealEstate.UpdateHousingPromotion;
using Kiwbi.Application.RealEstate.UpdateHousingPromotionMasterPlan;
using Kiwbi.Application.RealEstate.UpdateHousingTypology;
using Kiwbi.Application.RealEstate.UpdateHousingUnit;
using Kiwbi.Application.RealEstate.UpdateHousingUnitFloorPlan;
using Kiwbi.Application.Customizations.CreateTradeCategory;
using Kiwbi.Application.Customizations.UpdateTradeCategory;
using Kiwbi.Application.Customizations.DeleteTradeCategory;
using Kiwbi.Application.Customizations.GetTradeCategory;
using Kiwbi.Application.Customizations.GetTradeCategories;
using Kiwbi.Application.Customizations.CreateCustomization;
using Kiwbi.Application.Customizations.RenameCustomization;
using Kiwbi.Application.Customizations.DeleteCustomization;
using Kiwbi.Application.Customizations.GetCustomization;
using Kiwbi.Application.Customizations.GetCustomizations;
using Kiwbi.Application.Customizations.AssignCustomizationToTypology;
using Kiwbi.Application.Customizations.AssignCustomizationToUnit;
using Kiwbi.Application.Customizations.RemoveCustomizationAssignment;
using Kiwbi.Application.Customizations.AddCustomizationOption;
using Kiwbi.Application.Customizations.UpdateCustomizationOption;
using Kiwbi.Application.Customizations.SetDefaultCustomizationOption;
using Kiwbi.Application.Customizations.RemoveCustomizationOption;
using Kiwbi.Application.Onboarding.InviteBuyerToHousingUnit;
using Kiwbi.Application.Onboarding.ResendBuyerInvitation;
using Kiwbi.Application.Onboarding.CancelBuyerInvitation;
using Kiwbi.Application.Onboarding.GetBuyerInvitationsForHousingUnit;
using Kiwbi.Application.Onboarding.GetBuyerInvitationByToken;
using Kiwbi.Application.Onboarding.AcceptBuyerInvitation;
using Kiwbi.Application.Onboarding.GetHousingUnitBuyersForHousingUnit;
using Kiwbi.Application.Onboarding.GetHousingUnitsForCurrentBuyer;
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

        services.AddScoped<CreateHousingPromotionUseCase>();
        services.AddScoped<UpdateHousingPromotionUseCase>();
        services.AddScoped<UpdateHousingPromotionMasterPlanUseCase>();
        services.AddScoped<GetHousingPromotionUseCase>();
        services.AddScoped<GetHousingPromotionsUseCase>();
        services.AddScoped<GetHousingPromotionSummaryUseCase>();
        services.AddScoped<DeleteHousingPromotionUseCase>();

        services.AddScoped<CreateHousingTypologyUseCase>();
        services.AddScoped<UpdateHousingTypologyUseCase>();
        services.AddScoped<DeleteHousingTypologyUseCase>();
        services.AddScoped<GetHousingTypologyUseCase>();
        services.AddScoped<GetHousingTypologiesUseCase>();

        services.AddScoped<CreateHousingUnitUseCase>();
        services.AddScoped<UpdateHousingUnitUseCase>();
        services.AddScoped<UpdateHousingUnitFloorPlanUseCase>();
        services.AddScoped<ChangeHousingUnitStatusUseCase>();
        services.AddScoped<DeleteHousingUnitUseCase>();
        services.AddScoped<GetHousingUnitUseCase>();
        services.AddScoped<GetHousingUnitsUseCase>();

        services.AddScoped<CreateTradeCategoryUseCase>();
        services.AddScoped<UpdateTradeCategoryUseCase>();
        services.AddScoped<DeleteTradeCategoryUseCase>();
        services.AddScoped<GetTradeCategoryUseCase>();
        services.AddScoped<GetTradeCategoriesUseCase>();

        services.AddScoped<CreateCustomizationUseCase>();
        services.AddScoped<RenameCustomizationUseCase>();
        services.AddScoped<DeleteCustomizationUseCase>();
        services.AddScoped<GetCustomizationUseCase>();
        services.AddScoped<GetCustomizationsUseCase>();
        services.AddScoped<AssignCustomizationToTypologyUseCase>();
        services.AddScoped<AssignCustomizationToUnitUseCase>();
        services.AddScoped<RemoveCustomizationAssignmentUseCase>();

        services.AddScoped<AddCustomizationOptionUseCase>();
        services.AddScoped<UpdateCustomizationOptionUseCase>();
        services.AddScoped<SetDefaultCustomizationOptionUseCase>();
        services.AddScoped<RemoveCustomizationOptionUseCase>();

        services.AddScoped<InviteBuyerToHousingUnitUseCase>();
        services.AddScoped<ResendBuyerInvitationUseCase>();
        services.AddScoped<CancelBuyerInvitationUseCase>();
        services.AddScoped<GetBuyerInvitationsForHousingUnitUseCase>();
        services.AddScoped<GetBuyerInvitationByTokenUseCase>();
        services.AddScoped<AcceptBuyerInvitationUseCase>();
        services.AddScoped<GetHousingUnitBuyersForHousingUnitUseCase>();
        services.AddScoped<GetHousingUnitsForCurrentBuyerUseCase>();

        return services;
    }
}
