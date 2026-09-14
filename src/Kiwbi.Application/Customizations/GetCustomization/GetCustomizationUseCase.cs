using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.GetCustomization;

/// <summary>Resolves the detail of a Customization whose TradeCategory/HousingPromotion is owned by the currently authenticated tenant.</summary>
public class GetCustomizationUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;

    public GetCustomizationUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
    }

    public async Task<Result<CustomizationDto>> ExecuteAsync(Guid customizationId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<CustomizationDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var customization = await _customizationRepository.GetByIdAsync(customizationId, cancellationToken);

        if (customization is null)
        {
            return Result.Failure<CustomizationDto>("No se ha encontrado la personalización.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(customization.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure<CustomizationDto>("No se ha encontrado la personalización.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<CustomizationDto>("No se ha encontrado la personalización.");
        }

        return Result.Success(CustomizationDto.FromEntity(customization));
    }
}
