using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Choices.GetHousingUnitChoicesDetail;

/// <summary>Resolves, grouped by TradeCategory, the status and effective option of every applicable Customization of a HousingUnit for the promotora drill-down view (Feature 6.1).</summary>
public class GetHousingUnitChoicesDetailUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;

    public GetHousingUnitChoicesDetailUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
    }

    public async Task<Result<IReadOnlyList<TradeCategoryChoicesDto>>> ExecuteAsync(Guid housingUnitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryChoicesDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(housingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryChoicesDto>>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryChoicesDto>>("No se ha encontrado la vivienda.");
        }

        var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(unit.HousingPromotionId, cancellationToken);
        var choices = await _homeCustomizationChoiceRepository.GetByHousingUnitIdAsync(unit.Id, cancellationToken);
        var choicesByCustomization = choices.ToDictionary(c => c.CustomizationId);
        var utcNow = DateTime.UtcNow;
        var result = new List<TradeCategoryChoicesDto>();

        foreach (var tradeCategory in tradeCategories)
        {
            var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, cancellationToken);
            var isExpired = tradeCategory.IsExpired(utcNow);
            var applicableCustomizations = new List<CustomizationChoiceDto>();

            foreach (var customization in customizations)
            {
                if (!customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
                {
                    continue;
                }

                choicesByCustomization.TryGetValue(customization.Id, out var choice);
                applicableCustomizations.Add(HomeCustomizationChoiceProgressMapper.Build(customization, choice, isExpired));
            }

            if (applicableCustomizations.Count == 0)
            {
                continue;
            }

            result.Add(new TradeCategoryChoicesDto(
                tradeCategory.Id,
                tradeCategory.Name,
                tradeCategory.SelectionCutOffDateUtc,
                isExpired,
                applicableCustomizations));
        }

        return Result.Success<IReadOnlyList<TradeCategoryChoicesDto>>(result);
    }
}
