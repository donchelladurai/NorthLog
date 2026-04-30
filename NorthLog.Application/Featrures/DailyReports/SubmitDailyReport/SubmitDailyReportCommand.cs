namespace NorthLog.Application.Features.DailyReports.SubmitDailyReport;

public record SubmitDailyReportCommand(
    Guid WellboreId,
    DateOnly ReportDate,
    decimal DepthIn,
    decimal DepthOut,
    decimal TotalOilInBarrels,
    string LithologySummary,
    string? Notes);