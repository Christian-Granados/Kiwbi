using System.Reflection;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Application.Onboarding;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Kiwbi.Web.DemoSeeding;

/// <summary>
/// Seeds a demo tenant ("Kiwbi Demo") with 3 HousingPromotions covering the whole range of business states, for
/// manual verification/demo purposes. Idempotent: deletes any previous run of the same demo tenant before recreating it.
/// Reuses Domain factory/transition methods exactly like a real Application use case would - not a raw SQL/data dump.
/// </summary>
public class DemoDataSeeder
{
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountProvisioningService _accountProvisioningService;
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(
        IDeveloperCompanyRepository developerCompanyRepository,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IBuyerInvitationRepository buyerInvitationRepository,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IHomeCustomizationChoiceRepository homeCustomizationChoiceRepository,
        IUnitOfWork unitOfWork,
        IAccountProvisioningService accountProvisioningService,
        IBuyerAccountProvisioningService buyerAccountProvisioningService,
        UserManager<ApplicationUser> userManager,
        ILogger<DemoDataSeeder> logger)
    {
        _developerCompanyRepository = developerCompanyRepository;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _buyerInvitationRepository = buyerInvitationRepository;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _homeCustomizationChoiceRepository = homeCustomizationChoiceRepository;
        _unitOfWork = unitOfWork;
        _accountProvisioningService = accountProvisioningService;
        _buyerAccountProvisioningService = buyerAccountProvisioningService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await CleanupExistingDemoDataAsync(cancellationToken);

        _logger.LogInformation("Creando tenant de demo '{CompanyName}'...", DemoSeedingConstants.DeveloperCompanyName);

        var company = DeveloperCompany.Create(DemoSeedingConstants.DeveloperCompanyName);
        await _developerCompanyRepository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var adminResult = await _accountProvisioningService.CreateDeveloperAdminAccountAsync(
            DemoSeedingConstants.DeveloperAdminEmail, DemoSeedingConstants.DeveloperAdminPassword, company.Id, cancellationToken);

        if (adminResult.IsFailure)
        {
            throw new InvalidOperationException($"No se pudo crear la cuenta de la promotora demo: {adminResult.Error}");
        }

        await SeedResidencialVistalarAsync(company.Id, cancellationToken);
        await SeedResidencialPuertaAzulAsync(company.Id, cancellationToken);
        await SeedResidencialNuevosPinosAsync(company.Id, cancellationToken);

        _logger.LogInformation(
            "Seed de datos de demo completado: promotora '{Email}', compradores '{B1}'..'{B4}'.",
            DemoSeedingConstants.DeveloperAdminEmail,
            DemoSeedingConstants.Buyer1Email,
            DemoSeedingConstants.Buyer4Email);
    }

    // ---------------------------------------------------------------------
    // Idempotent cleanup of any previous demo run.
    // ---------------------------------------------------------------------

    private async Task CleanupExistingDemoDataAsync(CancellationToken cancellationToken)
    {
        var companies = await _developerCompanyRepository.GetAllAsync(cancellationToken);
        var existingCompany = companies.FirstOrDefault(c => c.Name == DemoSeedingConstants.DeveloperCompanyName);

        if (existingCompany is null)
        {
            return;
        }

        _logger.LogInformation("Eliminando datos de demo de una ejecución anterior...");

        var promotions = await _housingPromotionRepository.GetByDeveloperCompanyIdAsync(existingCompany.Id, cancellationToken);
        var buyerUserIds = new HashSet<string>();

        foreach (var promotion in promotions)
        {
            var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
            var unitIds = units.Select(u => u.Id).ToList();

            var choices = await _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(unitIds, cancellationToken);
            foreach (var choice in choices)
            {
                _homeCustomizationChoiceRepository.Remove(choice);
            }

            var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
            foreach (var tradeCategory in tradeCategories)
            {
                var customizations = await _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, cancellationToken);
                foreach (var customization in customizations)
                {
                    _customizationRepository.Remove(customization);
                }

                _tradeCategoryRepository.Remove(tradeCategory);
            }

            foreach (var unit in units)
            {
                var buyerLinks = await _housingUnitBuyerRepository.GetByHousingUnitIdAsync(unit.Id, cancellationToken);
                foreach (var link in buyerLinks)
                {
                    buyerUserIds.Add(link.BuyerUserId);
                    _housingUnitBuyerRepository.Remove(link);
                }

                var invitations = await _buyerInvitationRepository.GetByHousingUnitIdAsync(unit.Id, cancellationToken);
                foreach (var invitation in invitations)
                {
                    _buyerInvitationRepository.Remove(invitation);
                }

                _housingUnitRepository.Remove(unit);
            }

            var typologies = await _housingTypologyRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
            foreach (var typology in typologies)
            {
                _housingTypologyRepository.Remove(typology);
            }

            _housingPromotionRepository.Remove(promotion);
        }

        // One SaveChanges for the whole graph: EF Core topologically orders the DELETE statements by FK dependency
        // regardless of the order entities were marked as Removed above.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Identity users are deleted afterwards (in a separate UserManager-driven SaveChanges) since
        // home_buyer_assignments/developer_company_id both have a Restrict FK onto AspNetUsers - only safe now
        // that the rows referencing them have already been removed above.
        foreach (var buyerUserId in buyerUserIds)
        {
            var buyerUser = await _userManager.FindByIdAsync(buyerUserId);
            if (buyerUser is not null)
            {
                await _userManager.DeleteAsync(buyerUser);
            }
        }

        var adminUser = await _userManager.FindByEmailAsync(DemoSeedingConstants.DeveloperAdminEmail);
        if (adminUser is not null)
        {
            await _userManager.DeleteAsync(adminUser);
        }

        _developerCompanyRepository.Remove(existingCompany);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------------
    // Promoción 1: "Residencial Vistalar" - ACABADA / entregada.
    // ---------------------------------------------------------------------

    private async Task SeedResidencialVistalarAsync(Guid developerCompanyId, CancellationToken cancellationToken)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Vistalar", "Valencia", "Calle Mar Mediterráneo, 12");
        await _housingPromotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var typologyTwoBed = HousingTypology.Create(promotion.Id, "2 dormitorios");
        var typologyThreeBed = HousingTypology.Create(promotion.Id, "3 dormitorios");
        var typologyPenthouse = HousingTypology.Create(promotion.Id, "Ático");
        await _housingTypologyRepository.AddAsync(typologyTwoBed, cancellationToken);
        await _housingTypologyRepository.AddAsync(typologyThreeBed, cancellationToken);
        await _housingTypologyRepository.AddAsync(typologyPenthouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var unit1A = HousingUnit.Create(promotion.Id, "1", "A", 78m, 72m, typologyTwoBed.Id);
        var unit1B = HousingUnit.Create(promotion.Id, "1", "B", 80m, 74m, typologyTwoBed.Id);
        var unit2A = HousingUnit.Create(promotion.Id, "2", "A", 79m, 73m, typologyTwoBed.Id);
        var unit2B = HousingUnit.Create(promotion.Id, "2", "B", 81m, 75m, typologyTwoBed.Id);
        var unit3A = HousingUnit.Create(promotion.Id, "3", "A", 95m, 88m, typologyThreeBed.Id);
        var unit3B = HousingUnit.Create(promotion.Id, "3", "B", 96m, 89m, typologyThreeBed.Id);
        var unit4A = HousingUnit.Create(promotion.Id, "4", "A", 98m, 90m, typologyThreeBed.Id);
        var unit5A = HousingUnit.Create(promotion.Id, "5", "A", 120m, 105m, typologyPenthouse.Id);

        var allUnits = new[] { unit1A, unit1B, unit2A, unit2B, unit3A, unit3B, unit4A, unit5A };
        foreach (var unit in allUnits)
        {
            unit.ChangeStatus(HousingUnitStatus.Sold);
            await _housingUnitRepository.AddAsync(unit, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;
        var tradeCategoryAlicatados = TradeCategory.Create(promotion.Id, "Alicatados", utcNow.AddDays(-60));
        var tradeCategoryCarpinteria = TradeCategory.Create(promotion.Id, "Carpintería", utcNow.AddDays(-45));
        await _tradeCategoryRepository.AddAsync(tradeCategoryAlicatados, cancellationToken);
        await _tradeCategoryRepository.AddAsync(tradeCategoryCarpinteria, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sueloSalon = Customization.CreateForWholePromotion(tradeCategoryAlicatados.Id, "Suelo salón", "Roble natural", 0m);
        sueloSalon.AddOption("Roble oscuro", 450m);

        var azulejoBano = Customization.CreateForTypologies(tradeCategoryAlicatados.Id, "Azulejo baño", "Blanco mate", 0m, [typologyTwoBed.Id]);
        azulejoBano.AddOption("Gris antracita", 220m);

        var puertaEntrada = Customization.CreateForWholePromotion(tradeCategoryCarpinteria.Id, "Puerta de entrada", "Blindada estándar", 0m);
        puertaEntrada.AddOption("Blindada premium", 600m);

        var armarioEmpotrado = Customization.CreateForUnits(tradeCategoryCarpinteria.Id, "Armario empotrado", "Melamina blanca", 0m, [unit1A.Id, unit1B.Id]);
        armarioEmpotrado.AddOption("Melamina roble", 180m);

        await _customizationRepository.AddAsync(sueloSalon, cancellationToken);
        await _customizationRepository.AddAsync(azulejoBano, cancellationToken);
        await _customizationRepository.AddAsync(puertaEntrada, cancellationToken);
        await _customizationRepository.AddAsync(armarioEmpotrado, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Comprador 1 (1ºA): todo cerrado, todas sus elecciones en Paid.
        var buyer1UserId = await CreateBuyerAccountAsync(DemoSeedingConstants.Buyer1Email, cancellationToken);
        await LinkBuyerToUnitAsync(unit1A.Id, buyer1UserId, cancellationToken);
        foreach (var customization in new[] { sueloSalon, azulejoBano, puertaEntrada, armarioEmpotrado })
        {
            await AddChoiceAsync(unit1A.Id, customization.Id, NonDefaultOptionId(customization), HomeCustomizationChoiceStatus.Paid, cancellationToken);
        }

        // Comprador 2 (1ºB): la mayoría Paid, pero "Armario empotrado" se deja deliberadamente sin elección
        // (Pending sobre un Gremio ya vencido) para poder pulsar "Confirmar" en vivo durante la demo.
        var buyer2UserId = await CreateBuyerAccountAsync(DemoSeedingConstants.Buyer2Email, cancellationToken);
        await LinkBuyerToUnitAsync(unit1B.Id, buyer2UserId, cancellationToken);
        foreach (var customization in new[] { sueloSalon, azulejoBano, puertaEntrada })
        {
            await AddChoiceAsync(unit1B.Id, customization.Id, DefaultOptionId(customization), HomeCustomizationChoiceStatus.Paid, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------------
    // Promoción 2: "Residencial Puerta Azul" - CASI TERMINADA.
    // ---------------------------------------------------------------------

    private async Task SeedResidencialPuertaAzulAsync(Guid developerCompanyId, CancellationToken cancellationToken)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Puerta Azul", "Sevilla", "Avenida de la Constitución, 45");
        await _housingPromotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var typologyTwoBed = HousingTypology.Create(promotion.Id, "2 dormitorios");
        var typologyThreeBed = HousingTypology.Create(promotion.Id, "3 dormitorios");
        await _housingTypologyRepository.AddAsync(typologyTwoBed, cancellationToken);
        await _housingTypologyRepository.AddAsync(typologyThreeBed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var unit1A = HousingUnit.Create(promotion.Id, "1", "A", 76m, 70m, typologyTwoBed.Id);
        var unit1B = HousingUnit.Create(promotion.Id, "1", "B", 77m, 71m, typologyTwoBed.Id);
        var unit2A = HousingUnit.Create(promotion.Id, "2", "A", 92m, 85m, typologyThreeBed.Id);
        var unit2B = HousingUnit.Create(promotion.Id, "2", "B", 93m, 86m, typologyThreeBed.Id);
        var unit3A = HousingUnit.Create(promotion.Id, "3", "A", 78m, 72m, typologyTwoBed.Id);
        var unit3B = HousingUnit.Create(promotion.Id, "3", "B", 94m, 87m, typologyThreeBed.Id);

        unit1A.ChangeStatus(HousingUnitStatus.Sold);
        unit1B.ChangeStatus(HousingUnitStatus.Sold);
        unit2A.ChangeStatus(HousingUnitStatus.Sold);
        unit2B.ChangeStatus(HousingUnitStatus.Sold);
        unit3A.ChangeStatus(HousingUnitStatus.Reserved);
        unit3B.ChangeStatus(HousingUnitStatus.Reserved);

        foreach (var unit in new[] { unit1A, unit1B, unit2A, unit2B, unit3A, unit3B })
        {
            await _housingUnitRepository.AddAsync(unit, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;
        var tradeCategoryAlicatados = TradeCategory.Create(promotion.Id, "Alicatados", utcNow.AddDays(-40));
        var tradeCategoryCarpinteria = TradeCategory.Create(promotion.Id, "Carpintería", utcNow.AddDays(-20));
        var tradeCategoryElectricidad = TradeCategory.Create(promotion.Id, "Electricidad", utcNow.AddDays(30));
        await _tradeCategoryRepository.AddAsync(tradeCategoryAlicatados, cancellationToken);
        await _tradeCategoryRepository.AddAsync(tradeCategoryCarpinteria, cancellationToken);
        await _tradeCategoryRepository.AddAsync(tradeCategoryElectricidad, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sueloSalon = Customization.CreateForWholePromotion(tradeCategoryAlicatados.Id, "Suelo salón", "Roble natural", 0m);
        sueloSalon.AddOption("Roble oscuro", 400m);

        var griferiaBano = Customization.CreateForTypologies(tradeCategoryAlicatados.Id, "Grifería baño", "Cromado estándar", 0m, [typologyTwoBed.Id]);
        griferiaBano.AddOption("Cromado premium", 150m);

        var puertaEntrada = Customization.CreateForWholePromotion(tradeCategoryCarpinteria.Id, "Puerta de entrada", "Blindada estándar", 0m);
        puertaEntrada.AddOption("Blindada premium", 550m);

        var persianaMotorizada = Customization.CreateForUnits(tradeCategoryCarpinteria.Id, "Persiana motorizada", "Manual (incluida)", 0m, [unit2A.Id, unit2B.Id]);
        persianaMotorizada.AddOption("Motorizada con mando", 320m);

        var mecanismosElectricos = Customization.CreateForWholePromotion(tradeCategoryElectricidad.Id, "Mecanismos eléctricos", "Blanco estándar", 0m);
        mecanismosElectricos.AddOption("Serie premium negro", 280m);

        await _customizationRepository.AddAsync(sueloSalon, cancellationToken);
        await _customizationRepository.AddAsync(griferiaBano, cancellationToken);
        await _customizationRepository.AddAsync(puertaEntrada, cancellationToken);
        await _customizationRepository.AddAsync(persianaMotorizada, cancellationToken);
        await _customizationRepository.AddAsync(mecanismosElectricos, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Comprador 3 (2ºA): en curso, casi todo decidido - Confirmed en los gremios vencidos, Selected en el abierto.
        var buyer3UserId = await CreateBuyerAccountAsync(DemoSeedingConstants.Buyer3Email, cancellationToken);
        await LinkBuyerToUnitAsync(unit2A.Id, buyer3UserId, cancellationToken);
        await AddChoiceAsync(unit2A.Id, sueloSalon.Id, DefaultOptionId(sueloSalon), HomeCustomizationChoiceStatus.Confirmed, cancellationToken);
        await AddChoiceAsync(unit2A.Id, puertaEntrada.Id, NonDefaultOptionId(puertaEntrada), HomeCustomizationChoiceStatus.Confirmed, cancellationToken);
        await AddChoiceAsync(unit2A.Id, persianaMotorizada.Id, NonDefaultOptionId(persianaMotorizada), HomeCustomizationChoiceStatus.Selected, cancellationToken);
        await AddChoiceAsync(unit2A.Id, mecanismosElectricos.Id, DefaultOptionId(mecanismosElectricos), HomeCustomizationChoiceStatus.Selected, cancellationToken);

        // Comprador 4 (2ºB): el caso más variado - un gremio vencido ("Alicatados") sin decidir todavía,
        // el otro gremio vencido ("Carpintería") ya decidido, y el gremio abierto también sin decidir.
        var buyer4UserId = await CreateBuyerAccountAsync(DemoSeedingConstants.Buyer4Email, cancellationToken);
        await LinkBuyerToUnitAsync(unit2B.Id, buyer4UserId, cancellationToken);
        // sueloSalon (Alicatados, vencido): sin elección -> Pending con opción por defecto como efectiva.
        await AddChoiceAsync(unit2B.Id, puertaEntrada.Id, DefaultOptionId(puertaEntrada), HomeCustomizationChoiceStatus.Confirmed, cancellationToken);
        await AddChoiceAsync(unit2B.Id, persianaMotorizada.Id, NonDefaultOptionId(persianaMotorizada), HomeCustomizationChoiceStatus.Selected, cancellationToken);
        // mecanismosElectricos (Electricidad, abierto): sin elección -> Pending, sin opción efectiva (aún no vencido).

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3ºA (Reserved): invitación enviada hace tiempo, nunca aceptada, ya caducada.
        var expiredInvitation = BuyerInvitation.Create(unit3A.Id, "lead.caducado@kiwbi-demo.test");
        BackdateAsExpired(expiredInvitation, utcNow.AddDays(-20));
        await _buyerInvitationRepository.AddAsync(expiredInvitation, cancellationToken);

        // 3ºB (Reserved): invitación enviada recientemente, todavía pendiente (vigente).
        var pendingInvitation = BuyerInvitation.Create(unit3B.Id, "lead.pendiente@kiwbi-demo.test");
        await _buyerInvitationRepository.AddAsync(pendingInvitation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------------
    // Promoción 3: "Residencial Nuevos Pinos" - EN DEFINICIÓN.
    // ---------------------------------------------------------------------

    private async Task SeedResidencialNuevosPinosAsync(Guid developerCompanyId, CancellationToken cancellationToken)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Nuevos Pinos", "Zaragoza", "Paseo de la Independencia, 8");
        await _housingPromotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var typologyTwoBed = HousingTypology.Create(promotion.Id, "2 dormitorios");
        var typologyThreeBed = HousingTypology.Create(promotion.Id, "3 dormitorios");
        await _housingTypologyRepository.AddAsync(typologyTwoBed, cancellationToken);
        await _housingTypologyRepository.AddAsync(typologyThreeBed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var unit1A = HousingUnit.Create(promotion.Id, "1", "A", 74m, 68m, typologyTwoBed.Id);
        var unit1B = HousingUnit.Create(promotion.Id, "1", "B", 75m, 69m, typologyTwoBed.Id);
        var unit2A = HousingUnit.Create(promotion.Id, "2", "A", 90m, 83m, typologyThreeBed.Id);
        var unit2B = HousingUnit.Create(promotion.Id, "2", "B", 91m, 84m, typologyThreeBed.Id);
        var unit3A = HousingUnit.Create(promotion.Id, "3", "A", 92m, 85m, typologyThreeBed.Id);

        foreach (var unit in new[] { unit1A, unit1B, unit2A, unit2B, unit3A })
        {
            await _housingUnitRepository.AddAsync(unit, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tradeCategoryAlicatados = TradeCategory.Create(promotion.Id, "Alicatados", DateTime.UtcNow.AddDays(180));
        await _tradeCategoryRepository.AddAsync(tradeCategoryAlicatados, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sueloSalon = Customization.CreateForWholePromotion(tradeCategoryAlicatados.Id, "Suelo salón", "Roble natural", 0m);
        sueloSalon.AddOption("Roble oscuro", 400m);
        await _customizationRepository.AddAsync(sueloSalon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------------
    // Helpers.
    // ---------------------------------------------------------------------

    private static Guid DefaultOptionId(Customization customization) => customization.Options.First(o => o.IsDefault).Id;

    private static Guid NonDefaultOptionId(Customization customization) => customization.Options.First(o => !o.IsDefault).Id;

    private async Task<string> CreateBuyerAccountAsync(string email, CancellationToken cancellationToken)
    {
        var result = await _buyerAccountProvisioningService.CreateBuyerAccountAsync(email, DemoSeedingConstants.BuyerPassword, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"No se pudo crear la cuenta de comprador demo '{email}': {result.Error}");
        }

        return result.Value!;
    }

    private async Task LinkBuyerToUnitAsync(Guid housingUnitId, string buyerUserId, CancellationToken cancellationToken)
    {
        var link = HousingUnitBuyer.Create(housingUnitId, buyerUserId);
        await _housingUnitBuyerRepository.AddAsync(link, cancellationToken);
    }

    private async Task AddChoiceAsync(
        Guid housingUnitId,
        Guid customizationId,
        Guid selectedOptionId,
        HomeCustomizationChoiceStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var choice = HomeCustomizationChoice.Create(housingUnitId, customizationId);
        choice.SelectOption(selectedOptionId, utcNow);

        if (targetStatus is HomeCustomizationChoiceStatus.Confirmed or HomeCustomizationChoiceStatus.Paid)
        {
            choice.Confirm(utcNow);
        }

        if (targetStatus is HomeCustomizationChoiceStatus.Paid)
        {
            choice.MarkAsPaid(utcNow);
        }

        await _homeCustomizationChoiceRepository.AddAsync(choice, cancellationToken);
    }

    /// <summary>
    /// Backdates a just-created BuyerInvitation to simulate one sent long ago that's now expired. Domain doesn't expose
    /// a seeding-specific factory for this (BuyerInvitation.Create always stamps "now"), so this uses reflection over
    /// the private setters instead of adding demo-only surface area to Domain.
    /// </summary>
    private static void BackdateAsExpired(BuyerInvitation invitation, DateTime createdAtUtc)
    {
        SetPrivateProperty(invitation, nameof(BuyerInvitation.CreatedAtUtc), createdAtUtc);
        SetPrivateProperty(invitation, nameof(BuyerInvitation.UpdatedAtUtc), createdAtUtc);
        SetPrivateProperty(invitation, nameof(BuyerInvitation.ExpiresAtUtc), createdAtUtc.AddDays(7));
    }

    private static void SetPrivateProperty(object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"No se ha encontrado la propiedad '{propertyName}' en '{target.GetType().Name}'.");

        property.SetValue(target, value);
    }
}
