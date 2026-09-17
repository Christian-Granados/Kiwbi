using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Choices;

/// <summary>Entity relating a HousingUnit, a Customization and the CustomizationOption chosen by the buyer.</summary>
public class HomeCustomizationChoice : BaseEntity
{
    public Guid HousingUnitId { get; private set; }
    public Guid CustomizationId { get; private set; }
    public Guid? SelectedOptionId { get; private set; }
    public HomeCustomizationChoiceStatus Status { get; private set; }
    public DateTime? SelectedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private HomeCustomizationChoice()
    {
    }

    public static HomeCustomizationChoice Create(Guid housingUnitId, Guid customizationId)
    {
        if (housingUnitId == Guid.Empty)
        {
            throw new DomainException("La elección debe pertenecer a una vivienda.");
        }

        if (customizationId == Guid.Empty)
        {
            throw new DomainException("La elección debe pertenecer a una personalización.");
        }

        var choice = new HomeCustomizationChoice
        {
            HousingUnitId = housingUnitId,
            CustomizationId = customizationId,
            Status = HomeCustomizationChoiceStatus.Pending,
        };

        choice.CreatedAtUtc = DateTime.UtcNow;
        choice.UpdatedAtUtc = choice.CreatedAtUtc;

        return choice;
    }

    public void SelectOption(Guid customizationOptionId, DateTime utcNow)
    {
        if (customizationOptionId == Guid.Empty)
        {
            throw new DomainException("Debe indicarse la opción elegida.");
        }

        SelectedOptionId = customizationOptionId;
        Status = HomeCustomizationChoiceStatus.Selected;
        SelectedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Confirm(DateTime utcNow)
    {
        if (SelectedOptionId is null)
        {
            throw new DomainException("No se puede confirmar una elección sin una opción fijada.");
        }

        if (Status is HomeCustomizationChoiceStatus.Confirmed or HomeCustomizationChoiceStatus.Paid)
        {
            throw new DomainException("La elección ya ha sido confirmada.");
        }

        Status = HomeCustomizationChoiceStatus.Confirmed;
        UpdatedAtUtc = utcNow;
    }

    public void MarkAsPaid(DateTime utcNow)
    {
        if (Status != HomeCustomizationChoiceStatus.Confirmed)
        {
            throw new DomainException("Solo se puede marcar como pagada una elección ya confirmada.");
        }

        Status = HomeCustomizationChoiceStatus.Paid;
        UpdatedAtUtc = utcNow;
    }
}
