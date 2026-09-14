using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.ChangeHousingUnitStatus;

/// <summary>Changes the commercial status of a HousingUnit whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class ChangeHousingUnitStatusUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeHousingUnitStatusUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(ChangeHousingUnitStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.Id, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        unit.ChangeStatus(command.Status);

        _housingUnitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
