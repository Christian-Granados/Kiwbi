using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Application.Onboarding.AcceptBuyerInvitation;

/// <summary>
/// Accepts a pending BuyerInvitation by token: creates or reuses the buyer's Identity account, links it to the
/// HousingUnit via HousingUnitBuyer, marks the invitation Accepted and signs the buyer in. Anonymous by design: trusts
/// only the token, never a tenant/user context.
/// </summary>
public class AcceptBuyerInvitationUseCase
{
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptBuyerInvitationUseCase(
        IBuyerInvitationRepository buyerInvitationRepository,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IBuyerAccountProvisioningService buyerAccountProvisioningService,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork)
    {
        _buyerInvitationRepository = buyerInvitationRepository;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _buyerAccountProvisioningService = buyerAccountProvisioningService;
        _authenticationService = authenticationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(AcceptBuyerInvitationCommand command, CancellationToken cancellationToken = default)
    {
        var invitation = await _buyerInvitationRepository.GetByTokenAsync(command.Token, cancellationToken);

        if (invitation is null)
        {
            return Result.Failure("La invitación no existe o el enlace no es válido.");
        }

        var utcNow = DateTime.UtcNow;

        if (invitation.Status != BuyerInvitationStatus.Pending)
        {
            return Result.Failure("Esta invitación ya no está disponible.");
        }

        if (invitation.IsExpired(utcNow))
        {
            return Result.Failure("El enlace ha caducado. Pide a tu promotora que lo reenvíe.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var existingUserId = await _buyerAccountProvisioningService.FindUserIdByEmailAsync(invitation.Email, ct);
            string buyerUserId;
            var isNewAccount = existingUserId is null;

            if (existingUserId is not null)
            {
                var signInResult = await _authenticationService.SignInAsync(invitation.Email, command.Password, rememberMe: false, ct);

                if (signInResult.IsFailure)
                {
                    return signInResult;
                }

                buyerUserId = existingUserId;
            }
            else
            {
                var createResult = await _buyerAccountProvisioningService.CreateBuyerAccountAsync(invitation.Email, command.Password, ct);

                if (createResult.IsFailure)
                {
                    return Result.Failure(createResult.Error!);
                }

                buyerUserId = createResult.Value!;
            }

            if (!await _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(invitation.HousingUnitId, buyerUserId, ct))
            {
                var link = HousingUnitBuyer.Create(invitation.HousingUnitId, buyerUserId);
                await _housingUnitBuyerRepository.AddAsync(link, ct);
            }

            try
            {
                invitation.MarkAccepted(buyerUserId, utcNow);
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Message);
            }

            _buyerInvitationRepository.Update(invitation);
            await _unitOfWork.SaveChangesAsync(ct);

            if (isNewAccount)
            {
                var signInResult = await _authenticationService.SignInAsync(invitation.Email, command.Password, rememberMe: false, ct);

                if (signInResult.IsFailure)
                {
                    return signInResult;
                }
            }

            return Result.Success();
        }, cancellationToken);
    }
}
