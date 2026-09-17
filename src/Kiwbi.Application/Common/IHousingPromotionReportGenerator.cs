using Kiwbi.Application.Choices;

namespace Kiwbi.Application.Common;

/// <summary>Port for turning a HousingPromotionReportDto into downloadable file bytes, isolating Application from ClosedXML/QuestPDF (Feature 6.3).</summary>
public interface IHousingPromotionReportGenerator
{
    byte[] GenerateExcel(HousingPromotionReportDto report);

    byte[] GeneratePdf(HousingPromotionReportDto report);
}
