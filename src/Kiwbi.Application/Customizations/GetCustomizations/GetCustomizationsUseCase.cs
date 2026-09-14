using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.GetCustomizations;

/// <summary>Lists the Customizations of a TradeCategory owned by the currently authenticated tenant.</summary>
public class GetCustomizationsUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;

    public GetCustomizationsUseCase(
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

    public async Task<Result<IReadOnlyList<CustomizationDto>>> ExecuteAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<CustomizationDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(tradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure<IReadOnlyList<CustomizationDto>>("No se ha encontrado el gremio.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<CustomizationDto>>("No se ha encontrado el gremio.");
        }

        var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategoryId, cancellationToken);

        var dtos = customizations.Select(CustomizationDto.FromEntity).ToList();

        return Result.Success<IReadOnlyList<CustomizationDto>>(dtos);
    }
}
