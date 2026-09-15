using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.GetHousingUnitBuyersForHousingUnit;

/// <summary>Lists the buyers (confirmed HousingUnitBuyer links) of a HousingUnit owned by the currently authenticated tenant.</summary>
public class GetHousingUnitBuyersForHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService;

    public GetHousingUnitBuyersForHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IBuyerAccountProvisioningService buyerAccountProvisioningService)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _buyerAccountProvisioningService = buyerAccountProvisioningService;
    }

    public async Task<Result<IReadOnlyList<HousingUnitBuyerDto>>> ExecuteAsync(Guid housingUnitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingUnitBuyerDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(housingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<IReadOnlyList<HousingUnitBuyerDto>>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingUnitBuyerDto>>("No se ha encontrado la vivienda.");
        }

        var links = await _housingUnitBuyerRepository.GetByHousingUnitIdAsync(housingUnitId, cancellationToken);

        var dtos = new List<HousingUnitBuyerDto>();

        foreach (var link in links)
        {
            var email = await _buyerAccountProvisioningService.GetEmailByUserIdAsync(link.BuyerUserId, cancellationToken);
            dtos.Add(new HousingUnitBuyerDto(link.Id, link.HousingUnitId, link.BuyerUserId, email, link.CreatedAtUtc));
        }

        return Result.Success<IReadOnlyList<HousingUnitBuyerDto>>(dtos);
    }
}
