using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Domain.Customizations;

namespace Kiwbi.Application.Choices.SelectCustomizationOption;

/// <summary>Records the buyer's chosen CustomizationOption for a Customization applicable to their HousingUnit (Feature 5.3/5.4).</summary>
public class SelectCustomizationOptionUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SelectCustomizationOptionUseCase(
        ICurrentUser currentUser,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IHousingUnitRepository housingUnitRepository,
        ICustomizationRepository customizationRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _housingUnitRepository = housingUnitRepository;
        _customizationRepository = customizationRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomizationForBuyerDto>> ExecuteAsync(SelectCustomizationOptionCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } buyerUserId)
        {
            return Result.Failure<CustomizationForBuyerDto>("El usuario actual no ha iniciado sesión.");
        }

        var isLinkedToUnit = await _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(command.HousingUnitId, buyerUserId, cancellationToken);

        if (!isLinkedToUnit)
        {
            return Result.Failure<CustomizationForBuyerDto>("No se ha encontrado la vivienda.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.HousingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<CustomizationForBuyerDto>("No se ha encontrado la vivienda.");
        }

        var customization = await _customizationRepository.GetByIdAsync(command.CustomizationId, cancellationToken);

        if (customization is null || !customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
        {
            return Result.Failure<CustomizationForBuyerDto>("No se ha encontrado la personalización.");
        }

        if (customization.Options.All(o => o.Id != command.CustomizationOptionId))
        {
            return Result.Failure<CustomizationForBuyerDto>("No se ha encontrado la opción.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(customization.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure<CustomizationForBuyerDto>("No se ha encontrado la personalización.");
        }

        var utcNow = DateTime.UtcNow;

        if (tradeCategory.IsExpired(utcNow))
        {
            return Result.Failure<CustomizationForBuyerDto>("El plazo de selección para este gremio ha finalizado.");
        }

        var choice = await _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, cancellationToken);

        if (choice is null)
        {
            choice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
            choice.SelectOption(command.CustomizationOptionId, utcNow);
            await _homeCustomizationChoiceRepository.AddAsync(choice, cancellationToken);
        }
        else
        {
            choice.SelectOption(command.CustomizationOptionId, utcNow);
            _homeCustomizationChoiceRepository.Update(choice);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = CustomizationForBuyerMapper.Build(customization, choice, isExpired: false);

        return Result.Success(dto);
    }
}
