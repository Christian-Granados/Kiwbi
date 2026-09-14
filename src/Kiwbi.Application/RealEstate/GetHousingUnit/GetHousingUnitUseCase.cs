using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingUnit;

/// <summary>Resolves the detail of a HousingUnit whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class GetHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;

    public GetHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
    }

    public async Task<Result<HousingUnitDto>> ExecuteAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<HousingUnitDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(unitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<HousingUnitDto>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<HousingUnitDto>("No se ha encontrado la vivienda.");
        }

        return Result.Success(ToDto(unit));
    }

    internal static HousingUnitDto ToDto(Domain.RealEstate.HousingUnit unit) => new(
        unit.Id,
        unit.HousingPromotionId,
        unit.HousingTypologyId,
        unit.Floor,
        unit.Door,
        unit.BuiltAreaSqm,
        unit.UsableAreaSqm,
        unit.FloorPlanImagePath,
        unit.Status,
        unit.CreatedAtUtc,
        unit.UpdatedAtUtc);
}
