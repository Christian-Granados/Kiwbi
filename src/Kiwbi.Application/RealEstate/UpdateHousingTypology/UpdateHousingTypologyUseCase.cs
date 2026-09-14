using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.UpdateHousingTypology;

/// <summary>Renames a HousingTypology whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class UpdateHousingTypologyUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHousingTypologyUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateHousingTypologyCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var typology = await _housingTypologyRepository.GetByIdAsync(command.Id, cancellationToken);

        if (typology is null)
        {
            return Result.Failure("No se ha encontrado la tipología.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(typology.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la tipología.");
        }

        try
        {
            typology.Rename(command.Name);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _housingTypologyRepository.Update(typology);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
