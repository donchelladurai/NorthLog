using Microsoft.EntityFrameworkCore;
using NorthLog.Application.Common.Interfaces;

namespace NorthLog.Application.Features.Wellbores.GetWellboreDetails;

/* To the interviewer:
 This handler is deliberately bad. Two reasons throwing an exception here is
 the wrong tool for "wellbore not found":

 (1) "Wellbore not found" is not exceptional. It's an entirely expected
     outcome of a query — the caller passed an id, the database doesn't
     have it, that's a perfectly normal result. Exceptions should signal
     genuinely unexpected conditions: a corrupt database, a connection
     dropped mid-transaction, etc. Using exceptions for
     expected misses inverts the meaning of the word "exception".

 (2) Exceptions are expensive. 

 Also, the return type below isn't even Result<WellboreDetails> — it's
 just WellboreDetails. The signature itself lies: it implies the method
 always succeeds, when in fact it can throw. */

public static class GetWellboreDetailsHandler
{
    public static async Task<WellboreDetails> Handle(
        GetWellboreDetailsQuery request,
        IAppDbContext db,
        CancellationToken cancellationToken)
    {
        var info = await (
            from b in db.Wellbores
            join w in db.Wells on b.WellId equals w.Id
            join f in db.Fields on w.FieldId equals f.Id
            join o in db.Operators on f.OperatorId equals o.Id
            where b.Id == request.WellboreId
            select new
            {
                b.Id,
                b.Name,
                WellName = w.Name,
                FieldName = f.Name,
                OperatorName = o.Name,
                Status = b.Status.ToString(),
                b.KickoffDepthMeters
            }).FirstOrDefaultAsync(cancellationToken);

        // (deliberately bad)
        if (info is null)
            throw new WellboreNotFoundException(request.WellboreId);

        var reports = await db.DailyReports
            .Where(r => r.WellboreId == request.WellboreId)
            .OrderByDescending(r => r.ReportDate)
            .Select(r => new DailyReportRow(
                r.Id,
                r.ReportDate,
                r.DepthIn,
                r.DepthOut,
                r.DepthOut - r.DepthIn,
                r.TotalOilInBarrels,
                r.LithologySummary,
                r.Notes))
            .ToListAsync(cancellationToken);

        return new WellboreDetails(
            info.Id, info.Name, info.WellName, info.FieldName, info.OperatorName,
            info.Status, info.KickoffDepthMeters, reports);
    }
}