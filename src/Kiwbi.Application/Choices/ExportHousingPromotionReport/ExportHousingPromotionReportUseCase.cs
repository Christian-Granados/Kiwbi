using System.Text;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Choices.ExportHousingPromotionReport;

/// <summary>Builds the full "Libro de Obra" report (grouped by TradeCategory then HousingUnit) and delegates byte generation to IHousingPromotionReportGenerator (Feature 6.3).</summary>
public class ExportHousingPromotionReportUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;
    private readonly IHousingPromotionReportGenerator _reportGenerator;

    public ExportHousingPromotionReportUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository,
        IHousingPromotionReportGenerator reportGenerator)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
        _reportGenerator = reportGenerator;
    }

    public async Task<Result<GeneratedReportDto>> ExecuteAsync(ExportHousingPromotionReportCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<GeneratedReportDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(command.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<GeneratedReportDto>("No se ha encontrado la promoción.");
        }

        var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
        var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
        var choices = await _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(units.Select(u => u.Id), cancellationToken);
        var choicesByUnitAndCustomization = choices.ToDictionary(c => (c.HousingUnitId, c.CustomizationId));
        var utcNow = DateTime.UtcNow;

        var tradeCategoryDtos = new List<TradeCategoryReportDto>();

        foreach (var tradeCategory in tradeCategories)
        {
            var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, cancellationToken);
            var isExpired = tradeCategory.IsExpired(utcNow);
            var unitDtos = new List<HousingUnitReportDto>();

            foreach (var unit in units)
            {
                var applicableCustomizations = customizations
                    .Where(c => c.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId))
                    .Select(c =>
                    {
                        choicesByUnitAndCustomization.TryGetValue((unit.Id, c.Id), out var choice);
                        return HomeCustomizationChoiceProgressMapper.Build(c, choice, isExpired);
                    })
                    .ToList();

                if (applicableCustomizations.Count == 0)
                {
                    continue;
                }

                unitDtos.Add(new HousingUnitReportDto(unit.Floor, unit.Door, applicableCustomizations));
            }

            if (unitDtos.Count == 0)
            {
                continue;
            }

            tradeCategoryDtos.Add(new TradeCategoryReportDto(tradeCategory.Name, unitDtos));
        }

        var report = new HousingPromotionReportDto(promotion.Name, tradeCategoryDtos);

        var (content, extension, contentType) = command.Format switch
        {
            HousingPromotionReportFormat.Excel => (_reportGenerator.GenerateExcel(report), "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            HousingPromotionReportFormat.Pdf => (_reportGenerator.GeneratePdf(report), "pdf", "application/pdf"),
            _ => throw new ArgumentOutOfRangeException(nameof(command), "Formato de exportación no soportado."),
        };

        var fileName = $"libro-de-obra-{Sanitize(promotion.Name)}.{extension}";

        return Result.Success(new GeneratedReportDto(content, fileName, contentType));
    }

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        return builder.ToString();
    }
}
