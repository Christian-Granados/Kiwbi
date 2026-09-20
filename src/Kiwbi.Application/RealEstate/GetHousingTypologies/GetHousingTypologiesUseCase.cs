using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingTypologies;

/// <summary>Lists the HousingTypologies of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class GetHousingTypologiesUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;

    public GetHousingTypologiesUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
    }

    public async Task<Result<IReadOnlyList<HousingTypologyDto>>> ExecuteAsync(Guid housingPromotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingTypologyDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(housingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingTypologyDto>>("No se ha encontrado la promoción.");
        }

        var typologies = await _housingTypologyRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);
        var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);
        var unitCountsByTypologyId = units
            .Where(u => u.HousingTypologyId is not null)
            .GroupBy(u => u.HousingTypologyId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = typologies
            .Select(t => new HousingTypologyDto(
                t.Id, t.HousingPromotionId, t.Name, t.CreatedAtUtc, t.UpdatedAtUtc,
                unitCountsByTypologyId.GetValueOrDefault(t.Id)))
            .ToList();

        return Result.Success<IReadOnlyList<HousingTypologyDto>>(dtos);
    }
}
