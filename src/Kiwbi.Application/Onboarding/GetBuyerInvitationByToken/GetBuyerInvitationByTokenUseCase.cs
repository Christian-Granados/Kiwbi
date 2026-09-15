using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.GetBuyerInvitationByToken;

/// <summary>Resolves a BuyerInvitation by its Magic Link token for the anonymous acceptance screen. Trusts only the token, never a tenant/user context.</summary>
public class GetBuyerInvitationByTokenUseCase
{
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService;

    public GetBuyerInvitationByTokenUseCase(
        IBuyerInvitationRepository buyerInvitationRepository,
        IHousingUnitRepository housingUnitRepository,
        IHousingPromotionRepository housingPromotionRepository,
        IBuyerAccountProvisioningService buyerAccountProvisioningService)
    {
        _buyerInvitationRepository = buyerInvitationRepository;
        _housingUnitRepository = housingUnitRepository;
        _housingPromotionRepository = housingPromotionRepository;
        _buyerAccountProvisioningService = buyerAccountProvisioningService;
    }

    public async Task<Result<BuyerInvitationAcceptanceDto>> ExecuteAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitation = await _buyerInvitationRepository.GetByTokenAsync(token, cancellationToken);

        if (invitation is null)
        {
            return Result.Failure<BuyerInvitationAcceptanceDto>("La invitación no existe o el enlace no es válido.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(invitation.HousingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<BuyerInvitationAcceptanceDto>("La invitación no existe o el enlace no es válido.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);
        var accountAlreadyExists = await _buyerAccountProvisioningService.ExistsByEmailAsync(invitation.Email, cancellationToken);
        var utcNow = DateTime.UtcNow;

        var dto = new BuyerInvitationAcceptanceDto(
            unit.Id,
            invitation.Email,
            unit.Floor,
            unit.Door,
            promotion?.Name ?? string.Empty,
            invitation.Status,
            invitation.IsExpired(utcNow),
            accountAlreadyExists);

        return Result.Success(dto);
    }
}
