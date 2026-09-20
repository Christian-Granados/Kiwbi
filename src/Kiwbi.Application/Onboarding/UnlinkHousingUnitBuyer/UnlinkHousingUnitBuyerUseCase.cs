using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.UnlinkHousingUnitBuyer;

/// <summary>Removes a confirmed HousingUnitBuyer link, e.g. when a sale falls through or the unit is resold to someone else.
/// Never blocks on the existence of Confirmed/Paid HomeCustomizationChoice rows (Epic 11, Feature 11.3) - those are
/// preserved as-is and simply become inaccessible to the unlinked buyer; the Web layer warns the user before this
/// runs if that's the case, but the use case itself always proceeds once ownership is verified.</summary>
public class UnlinkHousingUnitBuyerUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlinkHousingUnitBuyerUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid housingUnitBuyerId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var link = await _housingUnitBuyerRepository.GetByIdAsync(housingUnitBuyerId, cancellationToken);

        if (link is null)
        {
            return Result.Failure("No se ha encontrado el vínculo con el comprador.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(link.HousingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado el vínculo con el comprador.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado el vínculo con el comprador.");
        }

        _housingUnitBuyerRepository.Remove(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
