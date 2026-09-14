using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.CreateHousingPromotion;

/// <summary>Creates a HousingPromotion owned by the DeveloperCompany of the currently authenticated user.</summary>
public class CreateHousingPromotionUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHousingPromotionUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> ExecuteAsync(CreateHousingPromotionCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<Guid>("El usuario actual no está vinculado a ninguna promotora.");
        }

        HousingPromotion promotion;

        try
        {
            promotion = HousingPromotion.Create(developerCompanyId, command.Name, command.City, command.Address);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        await _housingPromotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(promotion.Id);
    }
}
