using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingUnits;

/// <summary>Lists the HousingUnits of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class GetHousingUnitsUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;

    public GetHousingUnitsUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
    }

    public async Task<Result<IReadOnlyList<HousingUnitDto>>> ExecuteAsync(Guid housingPromotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingUnitDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(housingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingUnitDto>>("No se ha encontrado la promoción.");
        }

        var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);

        var dtos = units.Select(GetHousingUnitUseCase.ToDto).ToList();

        return Result.Success<IReadOnlyList<HousingUnitDto>>(dtos);
    }
}
