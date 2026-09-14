using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.CreateTradeCategory;

/// <summary>Creates a TradeCategory within a HousingPromotion owned by the currently authenticated tenant.</summary>
public class CreateTradeCategoryUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTradeCategoryUseCase(
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

    public async Task<Result<Guid>> ExecuteAsync(CreateTradeCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<Guid>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(command.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<Guid>("No se ha encontrado la promoción.");
        }

        TradeCategory tradeCategory;

        try
        {
            tradeCategory = TradeCategory.Create(command.HousingPromotionId, command.Name, command.SelectionCutOffDateUtc);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        await _tradeCategoryRepository.AddAsync(tradeCategory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tradeCategory.Id);
    }
}
