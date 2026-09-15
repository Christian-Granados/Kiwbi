namespace Kiwbi.Domain.Choices;

/// <summary>Lifecycle of a buyer's choice for a Customization. Confirmed/Paid are set later by the promotora (Epic 6).</summary>
public enum HomeCustomizationChoiceStatus
{
    Pending,
    Selected,
    Confirmed,
    Paid,
}
