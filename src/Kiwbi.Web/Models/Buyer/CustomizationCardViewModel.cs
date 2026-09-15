namespace Kiwbi.Web.Models.Buyer;

public class CustomizationCardViewModel
{
    public Guid HousingUnitId { get; set; }
    public CustomizationForBuyerViewModel Customization { get; set; } = null!;
}
