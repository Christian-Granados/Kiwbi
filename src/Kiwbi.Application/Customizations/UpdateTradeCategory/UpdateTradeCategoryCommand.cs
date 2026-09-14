namespace Kiwbi.Application.Customizations.UpdateTradeCategory;

public sealed record UpdateTradeCategoryCommand(Guid Id, string Name, DateTime SelectionCutOffDateUtc);
