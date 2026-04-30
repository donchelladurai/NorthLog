namespace NorthLog.Application.Features.Wellbores.GetWellboreDetails;

public record DailyReportRow(
    Guid Id,
    DateOnly ReportDate,
    decimal DepthIn,
    decimal DepthOut,
    decimal MetersDrilled,
    decimal TotalOilInBarrels,
    string LithologySummary);