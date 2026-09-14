using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.RenameCustomization;

/// <summary>Renames a Customization whose TradeCategory/HousingPromotion is owned by the currently authenticated tenant.</summary>
public class RenameCustomizationUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameCustomizationUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(RenameCustomizationCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var customization = await _customizationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (customization is null)
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        if (!await BelongsToTenantAsync(customization, developerCompanyId, cancellationToken))
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        try
        {
            customization.Rename(command.Name);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _customizationRepository.Update(customization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<bool> BelongsToTenantAsync(Customization customization, Guid developerCompanyId, CancellationToken cancellationToken)
    {
        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(customization.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return false;
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        return promotion is not null && promotion.DeveloperCompanyId == developerCompanyId;
    }
}
