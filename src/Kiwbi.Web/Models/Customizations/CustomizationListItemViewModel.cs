using Kiwbi.Application.Customizations;

namespace Kiwbi.Web.Models.Customizations;

public class CustomizationListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OptionsCount { get; set; }
    public string AssignmentsSummary { get; set; } = string.Empty;

    public static CustomizationListItemViewModel FromDto(CustomizationDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        OptionsCount = dto.Options.Count,
        AssignmentsSummary = CustomizationAssignmentSummary.Describe(dto.Assignments),
    };
}
