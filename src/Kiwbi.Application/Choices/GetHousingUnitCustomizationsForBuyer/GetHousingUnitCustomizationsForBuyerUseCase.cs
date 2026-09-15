using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Domain.Choices;

namespace Kiwbi.Application.Choices.GetHousingUnitCustomizationsForBuyer;

/// <summary>Resolves the Customizations applicable to a HousingUnit, grouped by TradeCategory, for the buyer linked to it (Feature 5.2).</summary>
public class GetHousingUnitCustomizationsForBuyerUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;

    public GetHousingUnitCustomizationsForBuyerUseCase(
        ICurrentUser currentUser,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository)
    {
        _currentUser = currentUser;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
    }

    public async Task<Result<IReadOnlyList<TradeCategoryCustomizationsDto>>> ExecuteAsync(Guid housingUnitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } buyerUserId)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryCustomizationsDto>>("El usuario actual no ha iniciado sesión.");
        }

        var isLinkedToUnit = await _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(housingUnitId, buyerUserId, cancellationToken);

        if (!isLinkedToUnit)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryCustomizationsDto>>("No se ha encontrado la vivienda.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(housingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryCustomizationsDto>>("No se ha encontrado la vivienda.");
        }

        var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(unit.HousingPromotionId, cancellationToken);
        var utcNow = DateTime.UtcNow;
        var result = new List<TradeCategoryCustomizationsDto>();

        foreach (var tradeCategory in tradeCategories)
        {
            var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, cancellationToken);
            var isExpired = tradeCategory.IsExpired(utcNow);
            var applicableCustomizations = new List<CustomizationForBuyerDto>();

            foreach (var customization in customizations)
            {
                if (!customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
                {
                    continue;
                }

                var choice = await _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, cancellationToken);
                applicableCustomizations.Add(CustomizationForBuyerMapper.Build(customization, choice, isExpired));
            }

            if (applicableCustomizations.Count == 0)
            {
                continue;
            }

            result.Add(new TradeCategoryCustomizationsDto(
                tradeCategory.Id,
                tradeCategory.Name,
                tradeCategory.SelectionCutOffDateUtc,
                isExpired,
                applicableCustomizations));
        }

        return Result.Success<IReadOnlyList<TradeCategoryCustomizationsDto>>(result);
    }
}
