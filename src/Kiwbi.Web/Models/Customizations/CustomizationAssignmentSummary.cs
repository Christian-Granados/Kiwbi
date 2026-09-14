using Kiwbi.Application.Customizations;
using Kiwbi.Domain.Customizations;

namespace Kiwbi.Web.Models.Customizations;

public static class CustomizationAssignmentSummary
{
    public static string Describe(IReadOnlyList<CustomizationAssignmentDto> assignments)
    {
        if (assignments.Count == 0)
        {
            return "Sin asignar";
        }

        return assignments[0].Scope switch
        {
            CustomizationScope.WholePromotion => "Toda la promoción",
            CustomizationScope.Typology => $"{assignments.Count} tipología(s)",
            CustomizationScope.Unit => $"{assignments.Count} vivienda(s)",
            _ => "Sin asignar",
        };
    }
}
