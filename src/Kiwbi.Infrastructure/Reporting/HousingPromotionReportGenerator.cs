using ClosedXML.Excel;
using Kiwbi.Application.Choices;
using Kiwbi.Application.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Kiwbi.Infrastructure.Reporting;

/// <summary>Renders a HousingPromotionReportDto into Excel (ClosedXML) or PDF (QuestPDF) bytes for the "Libro de Obra" export (Feature 6.3).</summary>
public class HousingPromotionReportGenerator : IHousingPromotionReportGenerator
{
    public byte[] GenerateExcel(HousingPromotionReportDto report)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Libro de Obra");

        worksheet.Cell(1, 1).Value = "Gremio";
        worksheet.Cell(1, 2).Value = "Planta";
        worksheet.Cell(1, 3).Value = "Puerta";
        worksheet.Cell(1, 4).Value = "Personalización";
        worksheet.Cell(1, 5).Value = "Opción efectiva";
        worksheet.Cell(1, 6).Value = "Sobrecoste";
        worksheet.Cell(1, 7).Value = "Estado";
        worksheet.Row(1).Style.Font.Bold = true;

        var row = 2;

        foreach (var tradeCategory in report.TradeCategories)
        {
            foreach (var unit in tradeCategory.HousingUnits)
            {
                foreach (var customization in unit.Customizations)
                {
                    worksheet.Cell(row, 1).Value = tradeCategory.TradeCategoryName;
                    worksheet.Cell(row, 2).Value = unit.Floor;
                    worksheet.Cell(row, 3).Value = unit.Door;
                    worksheet.Cell(row, 4).Value = customization.CustomizationName;
                    worksheet.Cell(row, 5).Value = customization.EffectiveOptionName ?? "-";
                    worksheet.Cell(row, 6).Value = customization.EffectiveOptionSurchargeAmount;
                    worksheet.Cell(row, 7).Value = customization.Status.ToString();
                    row++;
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    public byte[] GeneratePdf(HousingPromotionReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);

                page.Header().Text(report.PromotionName).FontSize(18).Bold();

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    foreach (var tradeCategory in report.TradeCategories)
                    {
                        column.Item().Text(tradeCategory.TradeCategoryName).FontSize(14).Bold();

                        foreach (var unit in tradeCategory.HousingUnits)
                        {
                            column.Item().Text($"Planta {unit.Floor}, puerta {unit.Door}").FontSize(11).SemiBold();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Personalización").Bold();
                                    header.Cell().Text("Opción efectiva").Bold();
                                    header.Cell().Text("Sobrecoste").Bold();
                                    header.Cell().Text("Estado").Bold();
                                });

                                foreach (var customization in unit.Customizations)
                                {
                                    table.Cell().Text(customization.CustomizationName);
                                    table.Cell().Text(customization.EffectiveOptionName ?? "-");
                                    table.Cell().Text(customization.EffectiveOptionSurchargeAmount is { } surcharge ? surcharge.ToString("C") : "-");
                                    table.Cell().Text(customization.Status.ToString());
                                }
                            });
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}
