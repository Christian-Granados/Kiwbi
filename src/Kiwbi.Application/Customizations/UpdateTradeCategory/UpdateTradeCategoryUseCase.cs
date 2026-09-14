using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.UpdateTradeCategory;

/// <summary>Renames/reschedules a TradeCategory whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class UpdateTradeCategoryUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTradeCategoryUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateTradeCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure("No se ha encontrado el gremio.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado el gremio.");
        }

        try
        {
            tradeCategory.Rename(command.Name);
            tradeCategory.Reschedule(command.SelectionCutOffDateUtc);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _tradeCategoryRepository.Update(tradeCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
