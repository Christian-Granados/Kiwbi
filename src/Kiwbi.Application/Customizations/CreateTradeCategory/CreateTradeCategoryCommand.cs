namespace Kiwbi.Application.Customizations.CreateTradeCategory;

public sealed record CreateTradeCategoryCommand(Guid HousingPromotionId, string Name, DateTime SelectionCutOffDateUtc);
