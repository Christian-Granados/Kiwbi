using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingPromotionSummary;

/// <summary>Resolves a HousingPromotion owned by the currently authenticated tenant together with the list of its units for the summary panel.</summary>
public class GetHousingPromotionSummaryUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;

    public GetHousingPromotionSummaryUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
    }

    public async Task<Result<HousingPromotionSummaryDto>> ExecuteAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<HousingPromotionSummaryDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(promotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<HousingPromotionSummaryDto>("No se ha encontrado la promoción.");
        }

        var typologies = await _housingTypologyRepository.GetByHousingPromotionIdAsync(promotionId, cancellationToken);
        var typologyNames = typologies.ToDictionary(t => t.Id, t => t.Name);

        var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(promotionId, cancellationToken);

        var unitSummaries = units
            .Select(u => new HousingUnitSummaryItemDto(
                u.Id,
                u.HousingTypologyId is { } typologyId ? typologyNames.GetValueOrDefault(typologyId) : null,
                u.Floor,
                u.Door,
                u.BuiltAreaSqm,
                u.UsableAreaSqm,
                u.Status))
            .ToList();

        var promotionDto = new HousingPromotionDto(
            promotion.Id,
            promotion.Name,
            promotion.City,
            promotion.Address,
            promotion.MasterPlanImagePath,
            promotion.CreatedAtUtc,
            promotion.UpdatedAtUtc);

        return Result.Success(new HousingPromotionSummaryDto(promotionDto, unitSummaries));
    }
}
