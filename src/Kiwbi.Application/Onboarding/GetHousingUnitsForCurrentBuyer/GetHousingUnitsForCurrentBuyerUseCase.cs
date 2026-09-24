using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.GetHousingUnitsForCurrentBuyer;

/// <summary>Lists the HousingUnits linked to the currently authenticated buyer via HousingUnitBuyer (Feature 5.1 dashboard).</summary>
public class GetHousingUnitsForCurrentBuyerUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingPromotionRepository _housingPromotionRepository;

    public GetHousingUnitsForCurrentBuyerUseCase(
        ICurrentUser currentUser,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IHousingUnitRepository housingUnitRepository,
        IHousingPromotionRepository housingPromotionRepository)
    {
        _currentUser = currentUser;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _housingUnitRepository = housingUnitRepository;
        _housingPromotionRepository = housingPromotionRepository;
    }

    public async Task<Result<IReadOnlyList<BuyerHousingUnitDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } buyerUserId)
        {
            return Result.Failure<IReadOnlyList<BuyerHousingUnitDto>>("El usuario actual no ha iniciado sesión.");
        }

        var links = await _housingUnitBuyerRepository.GetByBuyerUserIdAsync(buyerUserId, cancellationToken);

        var dtos = new List<BuyerHousingUnitDto>();

        foreach (var link in links)
        {
            var unit = await _housingUnitRepository.GetByIdAsync(link.HousingUnitId, cancellationToken);

            if (unit is null)
            {
                continue;
            }

            var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

            if (promotion is null)
            {
                continue;
            }

            dtos.Add(new BuyerHousingUnitDto(
                unit.Id,
                promotion.Id,
                promotion.Name,
                promotion.City,
                unit.Floor,
                unit.Door,
                unit.FloorPlanImagePath,
                promotion.MasterPlanImagePath));
        }

        return Result.Success<IReadOnlyList<BuyerHousingUnitDto>>(dtos);
    }
}
