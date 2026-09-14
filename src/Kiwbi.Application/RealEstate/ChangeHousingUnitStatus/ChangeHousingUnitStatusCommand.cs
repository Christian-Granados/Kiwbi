using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.ChangeHousingUnitStatus;

public sealed record ChangeHousingUnitStatusCommand(Guid Id, HousingUnitStatus Status);
