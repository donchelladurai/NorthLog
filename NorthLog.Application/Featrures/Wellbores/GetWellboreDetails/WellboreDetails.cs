namespace NorthLog.Application.Features.Wellbores.GetWellboreDetails;

public record WellboreDetails(
    Guid Id,
    string Name,
    string WellName,
    string FieldName,
    string OperatorName,
    string Status,
    decimal KickoffDepthMeters,
    IReadOnlyList<DailyReportRow> Reports);