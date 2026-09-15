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
}
