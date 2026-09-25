namespace Kiwbi.Application.Customizations.UpdateCustomizationOptionThumbnail;

public sealed record UpdateCustomizationOptionThumbnailCommand(Guid CustomizationId, Guid OptionId, Stream Content, string FileName);
