using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.GetHousingTypology;

/// <summary>Resolves the detail of a HousingTypology whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class GetHousingTypologyUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;

    public GetHousingTypologyUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
    }

    public async Task<Result<HousingTypologyDto>> ExecuteAsync(Guid typologyId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<HousingTypologyDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var typology = await _housingTypologyRepository.GetByIdAsync(typologyId, cancellationToken);

        if (typology is null)
        {
            return Result.Failure<HousingTypologyDto>("No se ha encontrado la tipología.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(typology.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<HousingTypologyDto>("No se ha encontrado la tipología.");
        }

        return Result.Success(new HousingTypologyDto(typology.Id, typology.HousingPromotionId, typology.Name, typology.CreatedAtUtc, typology.UpdatedAtUtc));
    }
}
