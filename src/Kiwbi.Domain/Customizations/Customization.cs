using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Customizations;

/// <summary>Aggregate root representing a Customization (Personalización) within a TradeCategory, owning its Options and Assignments.</summary>
public class Customization : BaseEntity
{
    private readonly List<CustomizationOption> _options = new();
    private readonly List<CustomizationAssignment> _assignments = new();

    public Guid TradeCategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CustomizationOption> Options => _options.AsReadOnly();
    public IReadOnlyCollection<CustomizationAssignment> Assignments => _assignments.AsReadOnly();

    private Customization()
    {
    }

    public static Customization CreateForWholePromotion(Guid tradeCategoryId, string name, string defaultOptionName, decimal defaultOptionSurchargeAmount)
    {
        var customization = CreateBase(tradeCategoryId, name, defaultOptionName, defaultOptionSurchargeAmount);
        customization._assignments.Add(CustomizationAssignment.ForWholePromotion());

        return customization;
    }

    public static Customization CreateForTypologies(Guid tradeCategoryId, string name, string defaultOptionName, decimal defaultOptionSurchargeAmount, IEnumerable<Guid> housingTypologyIds)
    {
        var ids = NormalizeTargetIds(housingTypologyIds, "tipología");
        var customization = CreateBase(tradeCategoryId, name, defaultOptionName, defaultOptionSurchargeAmount);

        foreach (var id in ids)
        {
            customization._assignments.Add(CustomizationAssignment.ForTypology(id));
        }

        return customization;
    }

    public static Customization CreateForUnits(Guid tradeCategoryId, string name, string defaultOptionName, decimal defaultOptionSurchargeAmount, IEnumerable<Guid> housingUnitIds)
    {
        var ids = NormalizeTargetIds(housingUnitIds, "vivienda");
        var customization = CreateBase(tradeCategoryId, name, defaultOptionName, defaultOptionSurchargeAmount);

        foreach (var id in ids)
        {
            customization._assignments.Add(CustomizationAssignment.ForUnit(id));
        }

        return customization;
    }

    private static Customization CreateBase(Guid tradeCategoryId, string name, string defaultOptionName, decimal defaultOptionSurchargeAmount)
    {
        if (tradeCategoryId == Guid.Empty)
        {
            throw new DomainException("La personalización debe pertenecer a un gremio.");
        }

        var customization = new Customization
        {
            TradeCategoryId = tradeCategoryId,
        };

        customization.Rename(name);
        customization._options.Add(new CustomizationOption(defaultOptionName, defaultOptionSurchargeAmount, isDefault: true));
        customization.CreatedAtUtc = customization.UpdatedAtUtc;

        return customization;
    }

    private static IReadOnlyList<Guid> NormalizeTargetIds(IEnumerable<Guid> ids, string targetLabel)
    {
        var distinctIds = (ids ?? Enumerable.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();

        if (distinctIds.Count == 0)
        {
            throw new DomainException($"Debe indicarse al menos una {targetLabel} de destino.");
        }

        return distinctIds;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la personalización es obligatorio.");
        }

        Name = name.Trim();
        Touch();
    }

    public void AddOption(string name, decimal surchargeAmount)
    {
        EnsureUniqueOptionName(name, excludingOptionId: null);
        _options.Add(new CustomizationOption(name, surchargeAmount, isDefault: false));
        Touch();
    }

    public void UpdateOption(Guid optionId, string name, decimal surchargeAmount)
    {
        var option = GetOptionOrThrow(optionId);
        EnsureUniqueOptionName(name, excludingOptionId: optionId);
        option.Rename(name);
        option.UpdateSurcharge(surchargeAmount);
        Touch();
    }

    public void SetDefaultOption(Guid optionId)
    {
        var option = GetOptionOrThrow(optionId);

        foreach (var existingOption in _options)
        {
            existingOption.UnmarkAsDefault();
        }

        option.MarkAsDefault();
        Touch();
    }

    public void RemoveOption(Guid optionId)
    {
        var option = GetOptionOrThrow(optionId);

        if (_options.Count == 1)
        {
            throw new DomainException("La personalización debe tener al menos una opción.");
        }

        if (option.IsDefault)
        {
            throw new DomainException("No se puede eliminar la opción por defecto; fija otra opción como predeterminada antes de eliminarla.");
        }

        _options.Remove(option);
        Touch();
    }

    private CustomizationOption GetOptionOrThrow(Guid optionId) =>
        _options.FirstOrDefault(o => o.Id == optionId) ?? throw new DomainException("No se ha encontrado la opción.");

    private void EnsureUniqueOptionName(string name, Guid? excludingOptionId)
    {
        var trimmedName = name?.Trim();

        if (_options.Any(o => o.Id != excludingOptionId && string.Equals(o.Name, trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("Ya existe una opción con ese nombre en esta personalización.");
        }
    }

    public void AssignToTypology(Guid housingTypologyId)
    {
        if (housingTypologyId == Guid.Empty)
        {
            throw new DomainException("La tipología de destino es obligatoria.");
        }

        EnsureCanAddSpecificAssignment();

        if (_assignments.Any(a => a.Scope == CustomizationScope.Typology && a.HousingTypologyId == housingTypologyId))
        {
            throw new DomainException("La personalización ya está asignada a esta tipología.");
        }

        _assignments.Add(CustomizationAssignment.ForTypology(housingTypologyId));
        Touch();
    }

    public void AssignToUnit(Guid housingUnitId)
    {
        if (housingUnitId == Guid.Empty)
        {
            throw new DomainException("La vivienda de destino es obligatoria.");
        }

        EnsureCanAddSpecificAssignment();

        if (_assignments.Any(a => a.Scope == CustomizationScope.Unit && a.HousingUnitId == housingUnitId))
        {
            throw new DomainException("La personalización ya está asignada a esta vivienda.");
        }

        _assignments.Add(CustomizationAssignment.ForUnit(housingUnitId));
        Touch();
    }

    private void EnsureCanAddSpecificAssignment()
    {
        if (_assignments.Any(a => a.Scope == CustomizationScope.WholePromotion))
        {
            throw new DomainException("No se pueden añadir asignaciones concretas si la personalización ya está asignada a toda la promoción.");
        }
    }

    public void RemoveAssignment(Guid assignmentId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId)
            ?? throw new DomainException("No se ha encontrado la asignación.");

        if (_assignments.Count == 1)
        {
            throw new DomainException("La personalización debe tener al menos una asignación.");
        }

        _assignments.Remove(assignment);
        Touch();
    }

    /// <summary>Whether this Customization applies to the given HousingUnit (and its optional HousingTypology), per its Assignments.</summary>
    public bool AppliesToHousingUnit(Guid housingUnitId, Guid? housingTypologyId) =>
        _assignments.Any(a =>
            a.Scope == CustomizationScope.WholePromotion ||
            (a.Scope == CustomizationScope.Typology && housingTypologyId is { } typologyId && a.HousingTypologyId == typologyId) ||
            (a.Scope == CustomizationScope.Unit && a.HousingUnitId == housingUnitId));

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
