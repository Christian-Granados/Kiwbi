using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.GetBuyerInvitationsForHousingUnit;

/// <summary>Lists the BuyerInvitations of a HousingUnit owned by the currently authenticated tenant.</summary>
public class GetBuyerInvitationsForHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;

    public GetBuyerInvitationsForHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IBuyerInvitationRepository buyerInvitationRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _buyerInvitationRepository = buyerInvitationRepository;
    }

    public async Task<Result<IReadOnlyList<BuyerInvitationDto>>> ExecuteAsync(Guid housingUnitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<BuyerInvitationDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(housingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<IReadOnlyList<BuyerInvitationDto>>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<BuyerInvitationDto>>("No se ha encontrado la vivienda.");
        }

        var invitations = await _buyerInvitationRepository.GetByHousingUnitIdAsync(housingUnitId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        var dtos = invitations
            .Select(i => new BuyerInvitationDto(i.Id, i.HousingUnitId, i.Email, i.Status, i.CreatedAtUtc, i.ExpiresAtUtc, i.IsExpired(utcNow), i.AcceptedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<BuyerInvitationDto>>(dtos);
    }
}
