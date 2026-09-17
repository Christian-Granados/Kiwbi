namespace Kiwbi.Application.Choices;

public sealed record HousingUnitChoicesProgressDto(
    Guid HousingUnitId,
    string? TypologyName,
    string Floor,
    string Door,
    int TotalCount,
    int PendingCount,
    int SelectedCount,
    int ConfirmedCount,
    int PaidCount);
