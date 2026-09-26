using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Onboarding.ResendBuyerInvitation;

/// <summary>Regenerates the token/expiration of a pending BuyerInvitation belonging to the currently authenticated tenant, and resends the email.</summary>
public class ResendBuyerInvitationUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public ResendBuyerInvitationUseCase(
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

    public async Task<Result> ExecuteAsync(Guid invitationId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var invitation = await _buyerInvitationRepository.GetByIdAsync(invitationId, cancellationToken);

        if (invitation is null)
        {
            return Result.Failure("No se ha encontrado la invitación.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(invitation.HousingUnitId, cancellationToken);
        var promotion = unit is null ? null : await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la invitación.");
        }

        try
        {
            invitation.Resend();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _buyerInvitationRepository.Update(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailSender.SendBuyerInvitationEmailAsync(invitation.Email, invitation.Token, invitation.ExpiresAtUtc, cancellationToken);
        }
        catch (FormatException)
        {
            return Result.Failure("El correo electrónico de esta invitación no es válido. Cancélala y crea una nueva con un correo electrónico corregido.");
        }

        return Result.Success();
    }
}
