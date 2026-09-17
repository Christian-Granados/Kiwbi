namespace Kiwbi.Application.Choices;

/// <summary>Full "Libro de Obra" export for a HousingPromotion, grouped by TradeCategory and then by HousingUnit (Feature 6.3).</summary>
public sealed record HousingPromotionReportDto(
    string PromotionName,
    IReadOnlyList<TradeCategoryReportDto> TradeCategories);
