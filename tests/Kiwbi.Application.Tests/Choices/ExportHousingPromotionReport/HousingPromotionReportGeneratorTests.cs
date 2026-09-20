using System.Text;
using ClosedXML.Excel;
using FluentAssertions;
using Kiwbi.Application.Choices;
using Kiwbi.Domain.Choices;
using Kiwbi.Infrastructure.Reporting;

namespace Kiwbi.Application.Tests.Choices.ExportHousingPromotionReport;

/// <summary>Exercises the real ClosedXML/QuestPDF generator (no mocks) - the mockable-generator tests in ExportHousingPromotionReportUseCaseTests can't catch real-mapping bugs (Features 10.2/10.3).</summary>
public class HousingPromotionReportGeneratorTests
{
    // Normally set once in AddInfrastructureServices; tests instantiate the generator directly, bypassing DI, so it must be set here too.
    static HousingPromotionReportGeneratorTests() => QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    private readonly HousingPromotionReportGenerator _generator = new();

    private static HousingPromotionReportDto BuildSampleReport() => new(
        "Residencial Acacias",
        [
            new TradeCategoryReportDto(
                "Carpintería",
                [
                    new HousingUnitReportDto(
                        "1",
                        "A",
                        [
                            new CustomizationChoiceDto(
                                Guid.NewGuid(),
                                "Puertas interiores",
                                HomeCustomizationChoiceStatus.Paid,
                                Guid.NewGuid(),
                                "Roble natural",
                                150.50m),
                            new CustomizationChoiceDto(
                                Guid.NewGuid(),
                                "Armario empotrado",
                                HomeCustomizationChoiceStatus.Pending,
                                null,
                                null,
                                null),
                        ]),
                ]),
        ]);

    [Fact]
    public void GenerateExcel_ShouldWriteHeaderRow()
    {
        var bytes = _generator.GenerateExcel(BuildSampleReport());

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Libro de Obra");

        worksheet.Cell(1, 1).GetString().Should().Be("Gremio");
        worksheet.Cell(1, 2).GetString().Should().Be("Planta");
        worksheet.Cell(1, 3).GetString().Should().Be("Puerta");
        worksheet.Cell(1, 4).GetString().Should().Be("Personalización");
        worksheet.Cell(1, 5).GetString().Should().Be("Opción efectiva");
        worksheet.Cell(1, 6).GetString().Should().Be("Sobrecoste");
        worksheet.Cell(1, 7).GetString().Should().Be("Estado");
    }

    [Fact]
    public void GenerateExcel_WithApplicableCustomizations_ShouldWriteOneRowPerCustomization()
    {
        var bytes = _generator.GenerateExcel(BuildSampleReport());

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Libro de Obra");

        worksheet.Cell(2, 1).GetString().Should().Be("Carpintería");
        worksheet.Cell(2, 2).GetString().Should().Be("1");
        worksheet.Cell(2, 3).GetString().Should().Be("A");
        worksheet.Cell(2, 4).GetString().Should().Be("Puertas interiores");
        worksheet.Cell(2, 5).GetString().Should().Be("Roble natural");
        worksheet.Cell(2, 6).GetValue<decimal>().Should().Be(150.50m);
        worksheet.Cell(2, 7).GetString().Should().Be("Paid");

        worksheet.Cell(3, 4).GetString().Should().Be("Armario empotrado");
        worksheet.Cell(3, 5).GetString().Should().Be("-");
        worksheet.Cell(3, 7).GetString().Should().Be("Pending");

        // No 4th data row: the sample report has exactly 2 customizations across a single unit/trade category.
        worksheet.Cell(4, 1).IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void GenerateExcel_WithoutEffectiveOption_ShouldWriteDashAndEmptySurcharge()
    {
        var bytes = _generator.GenerateExcel(BuildSampleReport());

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Libro de Obra");

        worksheet.Cell(3, 5).GetString().Should().Be("-");
        worksheet.Cell(3, 6).IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void GenerateExcel_WithEmptyReport_ShouldOnlyWriteHeaderRow()
    {
        var report = new HousingPromotionReportDto("Residencial Vacía", []);

        var bytes = _generator.GenerateExcel(report);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Libro de Obra");

        worksheet.Cell(1, 1).GetString().Should().Be("Gremio");
        worksheet.Cell(2, 1).IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void GeneratePdf_ShouldReturnNonEmptyBytesStartingWithThePdfMagicHeader()
    {
        var bytes = _generator.GeneratePdf(BuildSampleReport());

        bytes.Should().NotBeEmpty();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public void GeneratePdf_WithEmptyReport_ShouldStillGenerateAValidPdf()
    {
        var report = new HousingPromotionReportDto("Residencial Vacía", []);

        var bytes = _generator.GeneratePdf(report);

        bytes.Should().NotBeEmpty();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }
}
