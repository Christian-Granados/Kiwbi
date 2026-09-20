using FluentAssertions;
using Kiwbi.Application.Choices;
using Kiwbi.Application.Choices.ConfirmHomeCustomizationChoice;
using Kiwbi.Application.Choices.ExportHousingPromotionReport;
using Kiwbi.Application.Choices.GetHousingPromotionChoicesProgress;
using Kiwbi.Application.Choices.GetHousingUnitChoicesDetail;
using Kiwbi.Application.Choices.MarkHomeCustomizationChoiceAsPaid;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Kiwbi.Web.Tests.Controllers;

/// <summary>Controller-level coverage of HousingPromotionChoicesController.ExportExcel/ExportPdf (Features 10.2/10.3), no HTTP server involved.</summary>
public class HousingPromotionChoicesExportTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly IHousingPromotionReportGenerator _reportGenerator = Substitute.For<IHousingPromotionReportGenerator>();
    private readonly HousingPromotionChoicesController _controller;

    public HousingPromotionChoicesExportTests()
    {
        var exportUseCase = new ExportHousingPromotionReportUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository,
            _reportGenerator);

        // The other 4 use cases are only exercised by Index/Details/Confirm/MarkAsPaid, never by Export - trivially wired with mocked repos that are never invoked.
        var progressUseCase = new GetHousingPromotionChoicesProgressUseCase(
            _currentUser,
            _housingPromotionRepository,
            Substitute.For<IHousingTypologyRepository>(),
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository);

        var detailUseCase = new GetHousingUnitChoicesDetailUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository);

        var confirmUseCase = new ConfirmHomeCustomizationChoiceUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _customizationRepository,
            _tradeCategoryRepository,
            _homeCustomizationChoiceRepository,
            Substitute.For<IUnitOfWork>());

        var markAsPaidUseCase = new MarkHomeCustomizationChoiceAsPaidUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _homeCustomizationChoiceRepository,
            Substitute.For<IUnitOfWork>());

        var getPromotionUseCase = new GetHousingPromotionUseCase(_currentUser, _housingPromotionRepository);
        var getUnitUseCase = new GetHousingUnitUseCase(_currentUser, _housingPromotionRepository, _housingUnitRepository);

        _controller = new HousingPromotionChoicesController(
            progressUseCase,
            detailUseCase,
            confirmUseCase,
            markAsPaidUseCase,
            exportUseCase,
            getPromotionUseCase,
            getUnitUseCase);
    }

    private void SetupOwnedPromotionWithNoData(HousingPromotion promotion, Guid developerCompanyId)
    {
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _housingUnitRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns([]);
        _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns([]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);
    }

    [Fact]
    public async Task ExportExcel_WhenPromotionBelongsToAnotherTenant_ShouldReturnNotFound()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        SetupOwnedPromotionWithNoData(promotion, Guid.NewGuid());

        var result = await _controller.ExportExcel(promotion.Id, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ExportExcel_WhenPromotionIsOwned_ShouldReturnFileContentResultWithExcelBytes()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        SetupOwnedPromotionWithNoData(promotion, developerCompanyId);
        _reportGenerator.GenerateExcel(Arg.Any<HousingPromotionReportDto>()).Returns([1, 2, 3]);

        var result = await _controller.ExportExcel(promotion.Id, CancellationToken.None);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().Equal(1, 2, 3);
        fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileDownloadName.Should().EndWith(".xlsx");
    }

    [Fact]
    public async Task ExportPdf_WhenPromotionBelongsToAnotherTenant_ShouldReturnNotFound()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        SetupOwnedPromotionWithNoData(promotion, Guid.NewGuid());

        var result = await _controller.ExportPdf(promotion.Id, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ExportPdf_WhenPromotionIsOwned_ShouldReturnFileContentResultWithPdfBytes()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        SetupOwnedPromotionWithNoData(promotion, developerCompanyId);
        _reportGenerator.GeneratePdf(Arg.Any<HousingPromotionReportDto>()).Returns([9, 9, 9]);

        var result = await _controller.ExportPdf(promotion.Id, CancellationToken.None);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().Equal(9, 9, 9);
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().EndWith(".pdf");
    }
}
