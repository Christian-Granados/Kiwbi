using Kiwbi.Application.Common;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.UpdateHousingUnit;

/// <summary>Updates a HousingUnit whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class UpdateHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHousingUnitUseCase(
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

    public async Task<Result> ExecuteAsync(UpdateHousingUnitCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.Id, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        if (command.HousingTypologyId is { } typologyId)
        {
            var typology = await _housingTypologyRepository.GetByIdAsync(typologyId, cancellationToken);

            if (typology is null || typology.HousingPromotionId != unit.HousingPromotionId)
            {
                return Result.Failure("La tipología no pertenece a esta promoción.");
            }
        }

        var floor = command.Floor?.Trim() ?? string.Empty;
        var door = command.Door?.Trim() ?? string.Empty;

        if (await _housingUnitRepository.ExistsWithFloorAndDoorAsync(unit.HousingPromotionId, floor, door, unit.Id, cancellationToken))
        {
            return Result.Failure("Ya existe una vivienda con esa planta y puerta en esta promoción.");
        }

        try
        {
            unit.UpdateDetails(floor, door, command.BuiltAreaSqm, command.UsableAreaSqm);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        unit.AssignTypology(command.HousingTypologyId);

        _housingUnitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
