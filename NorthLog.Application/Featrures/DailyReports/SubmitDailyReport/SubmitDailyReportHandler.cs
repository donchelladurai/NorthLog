using Microsoft.EntityFrameworkCore;
using NorthLog.Application.Common;
using NorthLog.Application.Common.Interfaces;
using NorthLog.Domain.Exceptions;

namespace NorthLog.Application.Features.DailyReports.SubmitDailyReport;

/*   One of the reasons I've chosen Wolverine over Mediatr (happy to explain others in the interview).
     The Wolverine FluentValidation middleware runs SubmitDailyReportValidator
     before this method is invoked. If validation fails, the handler is never
     called and a ValidationException propagates back to the caller.
*/

public static class SubmitDailyReportHandler
{
    public static async Task<Result<Guid>> Handle(
        SubmitDailyReportCommand cmd,
        IAppDbContext db,
        CancellationToken ct)
    {
        var wellbore = await db.Wellbores
            .Include(b => b.Reports)
            .FirstOrDefaultAsync(b => b.Id == cmd.WellboreId, ct);

        /*  The good foil to GetWellboreDetailsHandler: same condition (wellbore
             doesn't exist), correct response (return a Result.Failure value).
             The ViewModel sees a structured value, not a thrown exception, and
             there's no stack-walk cost on what is an entirely expected failure. 
        */
        if (wellbore is null)
            return Result<Guid>.Failure($"Wellbore {cmd.WellboreId} was not found.");

        try
        {
            var report = wellbore.SubmitDailyReport(
                cmd.ReportDate,
                cmd.DepthIn,
                cmd.DepthOut,
                cmd.TotalOilInBarrels,
                cmd.LithologySummary,
                cmd.Notes);

            await db.SaveChangesAsync(ct);

            return report.Id;
        }
        catch (DomainException ex)
        {
            // Domain invariant violations are genuinely exceptional from the
            // domain's point of view — the aggregate uses exceptions to make
            // it impossible to silently corrupt state. At the application
            // boundary we translate them into a Result. Failure result here means the UI never
            // sees the DomainException type.

            return Result<Guid>.Failure(ex.Message);
        }
    }
}