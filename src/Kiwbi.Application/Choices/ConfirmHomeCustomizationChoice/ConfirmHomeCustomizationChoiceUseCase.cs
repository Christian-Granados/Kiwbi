using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Choices.ConfirmHomeCustomizationChoice;

/// <summary>Confirms (materializing the default option if the buyer never chose one) the promotora's acceptance of a Customization once its TradeCategory has expired (Feature 6.2).</summary>
public class ConfirmHomeCustomizationChoiceUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmHomeCustomizationChoiceUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        ICustomizationRepository customizationRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _customizationRepository = customizationRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(ConfirmHomeCustomizationChoiceCommand command, CancellationToken cancellationToken = default)
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

        var customization = await _customizationRepository.GetByIdAsync(command.CustomizationId, cancellationToken);

        if (customization is null || !customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(customization.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        var utcNow = DateTime.UtcNow;

        if (!tradeCategory.IsExpired(utcNow))
        {
            return Result.Failure("No se puede confirmar una personalización antes de que finalice el plazo de selección del gremio.");
        }

        var choice = await _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, cancellationToken);
        var isNewChoice = choice is null;
        choice ??= HomeCustomizationChoice.Create(unit.Id, customization.Id);

        try
        {
            if (choice.SelectedOptionId is null)
            {
                var defaultOptionId = customization.Options.First(o => o.IsDefault).Id;
                choice.SelectOption(defaultOptionId, utcNow);
            }

            choice.Confirm(utcNow);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        if (isNewChoice)
        {
            await _homeCustomizationChoiceRepository.AddAsync(choice, cancellationToken);
        }
        else
        {
            _homeCustomizationChoiceRepository.Update(choice);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
