using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingPromotion;

/// <summary>Resolves the detail of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class GetHousingPromotionUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;

    public GetHousingPromotionUseCase(ICurrentUser currentUser, IHousingPromotionRepository housingPromotionRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
    }

    public async Task<Result<HousingPromotionDto>> ExecuteAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<HousingPromotionDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(promotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<HousingPromotionDto>("No se ha encontrado la promoción.");
        }

        return Result.Success(new HousingPromotionDto(
            promotion.Id,
            promotion.Name,
            promotion.City,
            promotion.Address,
            promotion.MasterPlanImagePath,
            promotion.CreatedAtUtc,
            promotion.UpdatedAtUtc));
    }
}
