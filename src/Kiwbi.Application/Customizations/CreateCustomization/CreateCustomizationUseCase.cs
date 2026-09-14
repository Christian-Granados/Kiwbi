using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.CreateCustomization;

/// <summary>Creates a Customization within a TradeCategory owned by the currently authenticated tenant, with its initial default option and assignment.</summary>
public class CreateCustomizationUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomizationUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> ExecuteAsync(CreateCustomizationCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<Guid>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(command.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure<Guid>("No se ha encontrado el gremio.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<Guid>("No se ha encontrado el gremio.");
        }

        Customization? customization;

        try
        {
            customization = command.Scope switch
            {
                CustomizationScope.WholePromotion => Customization.CreateForWholePromotion(
                    command.TradeCategoryId, command.Name, command.DefaultOptionName, command.DefaultOptionSurchargeAmount),
                CustomizationScope.Typology => await CreateForTypologiesAsync(command, promotion.Id, cancellationToken),
                CustomizationScope.Unit => await CreateForUnitsAsync(command, promotion.Id, cancellationToken),
                _ => throw new DomainException("El nivel de asignación de la personalización no es válido."),
            };
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        if (customization is null)
        {
            return Result.Failure<Guid>("Una o más tipologías o viviendas no pertenecen a la promoción del gremio.");
        }

        await _customizationRepository.AddAsync(customization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(customization.Id);
    }

    private async Task<Customization?> CreateForTypologiesAsync(CreateCustomizationCommand command, Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var promotionTypologies = await _housingTypologyRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);
        var promotionTypologyIds = promotionTypologies.Select(t => t.Id).ToHashSet();

        if (command.HousingTypologyIds.Any(id => !promotionTypologyIds.Contains(id)))
        {
            return null;
        }

        return Customization.CreateForTypologies(command.TradeCategoryId, command.Name, command.DefaultOptionName, command.DefaultOptionSurchargeAmount, command.HousingTypologyIds);
    }

    private async Task<Customization?> CreateForUnitsAsync(CreateCustomizationCommand command, Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var promotionUnits = await _housingUnitRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);
        var promotionUnitIds = promotionUnits.Select(u => u.Id).ToHashSet();

        if (command.HousingUnitIds.Any(id => !promotionUnitIds.Contains(id)))
        {
            return null;
        }

        return Customization.CreateForUnits(command.TradeCategoryId, command.Name, command.DefaultOptionName, command.DefaultOptionSurchargeAmount, command.HousingUnitIds);
    }
}
