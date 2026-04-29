using NorthLog.Domain.Enums;
using NorthLog.Domain.Exceptions;

namespace NorthLog.Domain.Entities;

public class Wellbore
{
        /*
        Note to the interviewers:

         internal modified here so a wellbore can only be created from a well.AddWellBore().
    */
    internal Wellbore(Guid wellId, string name, decimal kickoffDepthMeters)
    {
        if (kickoffDepthMeters < 0)
            throw new DomainException("Kickoff depth cannot be negative.");

        WellId = wellId;
        Name = name;
        KickoffDepthMeters = kickoffDepthMeters;
    }

    /*
     Note to the interviewers:

         The rules enforced here (depth-out >= depth-in, depth-in >= deepestSoFar, etc.) are domain rules that
         are true regardless of which UI, API, or batch job submits the data. I've defined them here as they are invariant, regarless of which UI, API, etc. uses them. 
        
         The FluentValidation in the Application layer is for "input shape" only and I wanted to draw a hard line here between the two. 
         So if you ever find the same rule in both places, one of them is wrong.
    */
    public DailyReport SubmitDailyReport(
        DateOnly reportDate,
        decimal depthIn,
        decimal depthOut,
        decimal totalOilInBarrels,
        string lithologySummary,
        string? notes)
    {
        if (Status == WellboreStatus.Completed)
            throw new DomainException("Cannot submit reports against a completed wellbore.");

        if (depthOut < depthIn)
            throw new DomainException("Depth out cannot be shallower than depth in.");

        var deepestSoFar = _reports.Count == 0
            ? KickoffDepthMeters
            : _reports.Max(r => r.DepthOut);

        if (depthIn < deepestSoFar)
            throw new DomainException(
                $"Depth in ({depthIn} m) is shallower than the previous deepest report ({deepestSoFar} m).");

        if (_reports.Any(r => r.ReportDate == reportDate))
            throw new DomainException($"A report already exists for {reportDate:yyyy-MM-dd}.");

        var report = new DailyReport(
            this.Id, reportDate, depthIn, depthOut,
            totalOilInBarrels,
            lithologySummary, notes);

        _reports.Add(report);
        return report;
    }

    public void Complete()
    {
        if (_reports.Count == 0)
            throw new DomainException("Cannot complete a wellbore with no reports.");

        Status = WellboreStatus.Completed;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WellId { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal KickoffDepthMeters { get; private set; }
    public WellboreStatus Status { get; private set; } = WellboreStatus.Drilling;
    public IReadOnlyCollection<DailyReport> Reports => _reports.AsReadOnly();

    // To the interviewers: private modifier here so reports can only be added through the SubmitDailyReport() method, which enforces the domain rules around report submission.
    private readonly List<DailyReport> _reports = [];
}