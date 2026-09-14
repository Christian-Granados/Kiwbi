using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingPromotions;

/// <summary>Lists the HousingPromotions owned by the currently authenticated tenant.</summary>
public class GetHousingPromotionsUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;

    public GetHousingPromotionsUseCase(ICurrentUser currentUser, IHousingPromotionRepository housingPromotionRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
    }

    public async Task<Result<IReadOnlyList<HousingPromotionDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<HousingPromotionDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotions = await _housingPromotionRepository.GetByDeveloperCompanyIdAsync(developerCompanyId, cancellationToken);

        var dtos = promotions
            .Select(p => new HousingPromotionDto(p.Id, p.Name, p.City, p.Address, p.MasterPlanImagePath, p.CreatedAtUtc, p.UpdatedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<HousingPromotionDto>>(dtos);
    }
}
