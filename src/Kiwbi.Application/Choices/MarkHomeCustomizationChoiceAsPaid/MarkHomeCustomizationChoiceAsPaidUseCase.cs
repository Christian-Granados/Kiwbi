using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Choices.MarkHomeCustomizationChoiceAsPaid;

/// <summary>Marks an already-Confirmed HomeCustomizationChoice as Paid once the surcharge has been collected from the buyer (Feature 6.2).</summary>
public class MarkHomeCustomizationChoiceAsPaidUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkHomeCustomizationChoiceAsPaidUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(MarkHomeCustomizationChoiceAsPaidCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.HousingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var choice = await _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, command.CustomizationId, cancellationToken);

        if (choice is null)
        {
            return Result.Failure("No se puede marcar como pagada una elección que no ha sido confirmada.");
        }

        try
        {
            choice.MarkAsPaid(DateTime.UtcNow);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _homeCustomizationChoiceRepository.Update(choice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
