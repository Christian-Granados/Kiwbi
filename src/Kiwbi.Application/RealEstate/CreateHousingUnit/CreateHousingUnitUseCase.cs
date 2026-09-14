using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.CreateHousingUnit;

/// <summary>Creates a HousingUnit within a HousingPromotion owned by the currently authenticated tenant.</summary>
public class CreateHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> ExecuteAsync(CreateHousingUnitCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<Guid>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(command.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<Guid>("No se ha encontrado la promoción.");
        }

        if (command.HousingTypologyId is { } typologyId)
        {
            var typology = await _housingTypologyRepository.GetByIdAsync(typologyId, cancellationToken);

            if (typology is null || typology.HousingPromotionId != command.HousingPromotionId)
            {
                return Result.Failure<Guid>("La tipología no pertenece a esta promoción.");
            }
        }

        var floor = command.Floor?.Trim() ?? string.Empty;
        var door = command.Door?.Trim() ?? string.Empty;

        if (await _housingUnitRepository.ExistsWithFloorAndDoorAsync(command.HousingPromotionId, floor, door, cancellationToken: cancellationToken))
        {
            return Result.Failure<Guid>("Ya existe una vivienda con esa planta y puerta en esta promoción.");
        }

        HousingUnit unit;

        try
        {
            unit = HousingUnit.Create(command.HousingPromotionId, floor, door, command.BuiltAreaSqm, command.UsableAreaSqm, command.HousingTypologyId);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        await _housingUnitRepository.AddAsync(unit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(unit.Id);
    }
}
