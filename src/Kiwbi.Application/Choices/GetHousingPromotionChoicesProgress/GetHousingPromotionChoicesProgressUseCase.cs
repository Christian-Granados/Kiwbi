using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Choices.GetHousingPromotionChoicesProgress;

/// <summary>Resolves, per HousingUnit of a HousingPromotion owned by the current tenant, the counts of applicable Customizations by choice status (Feature 6.1).</summary>
public class GetHousingPromotionChoicesProgressUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;

    public GetHousingPromotionChoicesProgressUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
    }

    public async Task<Result<HousingPromotionChoicesProgressDto>> ExecuteAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<HousingPromotionChoicesProgressDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(promotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<HousingPromotionChoicesProgressDto>("No se ha encontrado la promoción.");
        }

        var typologies = await _housingTypologyRepository.GetByHousingPromotionIdAsync(promotionId, cancellationToken);
        var typologyNames = typologies.ToDictionary(t => t.Id, t => t.Name);

        var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(promotionId, cancellationToken);
        var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotionId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        var applicableCustomizations = new List<(Customization Customization, bool IsExpired)>();

        foreach (var tradeCategory in tradeCategories)
        {
            var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, cancellationToken);
            var isExpired = tradeCategory.IsExpired(utcNow);

            applicableCustomizations.AddRange(customizations.Select(c => (c, isExpired)));
        }

        var choices = await _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(units.Select(u => u.Id), cancellationToken);
        var choicesByUnitAndCustomization = choices.ToDictionary(c => (c.HousingUnitId, c.CustomizationId));

        var unitDtos = new List<HousingUnitChoicesProgressDto>();

        foreach (var unit in units)
        {
            var pendingCount = 0;
            var selectedCount = 0;
            var confirmedCount = 0;
            var paidCount = 0;

            foreach (var (customization, isExpired) in applicableCustomizations)
            {
                if (!customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
                {
                    continue;
                }

                choicesByUnitAndCustomization.TryGetValue((unit.Id, customization.Id), out var choice);
                var status = choice?.Status ?? HomeCustomizationChoiceStatus.Pending;

                switch (status)
                {
                    case HomeCustomizationChoiceStatus.Pending:
                        pendingCount++;
                        break;
                    case HomeCustomizationChoiceStatus.Selected:
                        selectedCount++;
                        break;
                    case HomeCustomizationChoiceStatus.Confirmed:
                        confirmedCount++;
                        break;
                    case HomeCustomizationChoiceStatus.Paid:
                        paidCount++;
                        break;
                }
            }

            unitDtos.Add(new HousingUnitChoicesProgressDto(
                unit.Id,
                unit.HousingTypologyId is { } typologyId ? typologyNames.GetValueOrDefault(typologyId) : null,
                unit.Floor,
                unit.Door,
                pendingCount + selectedCount + confirmedCount + paidCount,
                pendingCount,
                selectedCount,
                confirmedCount,
                paidCount));
        }

        return Result.Success(new HousingPromotionChoicesProgressDto(unitDtos));
    }
}
