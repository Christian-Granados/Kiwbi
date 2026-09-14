using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.UpdateHousingPromotion;

/// <summary>Updates the details of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class UpdateHousingPromotionUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHousingPromotionUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateHousingPromotionCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la promoción.");
        }

        try
        {
            promotion.UpdateDetails(command.Name, command.City, command.Address);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _housingPromotionRepository.Update(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
