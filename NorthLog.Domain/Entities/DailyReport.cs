namespace NorthLog.Domain.Entities;

public class DailyReport
{
    internal DailyReport() { }

    internal DailyReport(
        Guid wellboreId, 
        DateOnly reportDate,
        decimal depthIn, 
        decimal depthOut,
        decimal totalOilInBarrels,
        string lithology, 
        string? notes)
    {
        WellboreId = wellboreId;
        ReportDate = reportDate;
        DepthIn = depthIn;
        DepthOut = depthOut;
        TotalOilInBarrels = totalOilInBarrels;
        LithologySummary = lithology;
        Notes = notes;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WellboreId { get; private set; }
    public DateOnly ReportDate { get; private set; }
    public decimal DepthIn { get; }
    public decimal DepthOut { get; }
    public decimal TotalOilInBarrels { get; private set; }
    public string LithologySummary { get; private set; } = default!;
    public string? Notes { get; private set; }
    public decimal MetersDrilled => DepthOut - DepthIn;
}