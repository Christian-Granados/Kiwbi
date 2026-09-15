using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.InviteBuyerToHousingUnit;

/// <summary>Creates a pending BuyerInvitation for a HousingUnit owned by the currently authenticated tenant and emails the Magic Link.</summary>
public class InviteBuyerToHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public InviteBuyerToHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IBuyerInvitationRepository buyerInvitationRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _buyerInvitationRepository = buyerInvitationRepository;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> ExecuteAsync(InviteBuyerToHousingUnitCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<Guid>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.HousingUnitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<Guid>("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<Guid>("No se ha encontrado la vivienda.");
        }

        var email = command.Email?.Trim() ?? string.Empty;

        if (await _buyerInvitationRepository.ExistsPendingByHousingUnitIdAndEmailAsync(command.HousingUnitId, email, cancellationToken))
        {
            return Result.Failure<Guid>("Ya existe una invitación pendiente para ese correo en esta vivienda.");
        }

        BuyerInvitation invitation;

        try
        {
            invitation = BuyerInvitation.Create(command.HousingUnitId, email);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        await _buyerInvitationRepository.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailSender.SendBuyerInvitationEmailAsync(invitation.Email, invitation.Token, invitation.ExpiresAtUtc, cancellationToken);

        return Result.Success(invitation.Id);
    }
}
