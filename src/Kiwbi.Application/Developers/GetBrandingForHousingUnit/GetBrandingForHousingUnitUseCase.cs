using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Developers.GetBrandingForHousingUnit;

/// <summary>Resolves the branding of the DeveloperCompany that owns the HousingPromotion of a given HousingUnit. Used to accent-brand the Buyer-facing HousingUnit detail view (Epic 8), where the tenant is otherwise ambiguous.</summary>
public class GetBrandingForHousingUnitUseCase
{
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;

    public GetBrandingForHousingUnitUseCase(
        IHousingUnitRepository housingUnitRepository,
        IHousingPromotionRepository housingPromotionRepository,
        IDeveloperCompanyRepository developerCompanyRepository)
    {
        _housingUnitRepository = housingUnitRepository;
        _housingPromotionRepository = housingPromotionRepository;
        _developerCompanyRepository = developerCompanyRepository;
    }

    public async Task<Result<DeveloperProfileDto>> ExecuteAsync(Guid housingUnitId, CancellationToken cancellationToken = default)
    {
        var unit = await _housingUnitRepository.GetByIdAsync(housingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<DeveloperProfileDto>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null)
        {
            return Result.Failure<DeveloperProfileDto>("No se ha encontrado la promoción.");
        }

        var company = await _developerCompanyRepository.GetByIdAsync(promotion.DeveloperCompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure<DeveloperProfileDto>("No se ha encontrado la promotora.");
        }

        return Result.Success(GetCurrentDeveloperProfileUseCase.ToDto(company));
    }
}
