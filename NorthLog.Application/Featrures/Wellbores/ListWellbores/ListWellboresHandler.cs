using Microsoft.EntityFrameworkCore;
using NorthLog.Application.Common;
using NorthLog.Application.Common.Interfaces;
using NorthLog.Application.Featrures.Wellbores;

namespace NorthLog.Application.Features.Wellbores.ListWellbores;

public static class ListWellboresHandler
{
    public static async Task<Result<IReadOnlyList<WellboreListItem>>> Handle(
        ListWellboresQuery query,
        IAppDbContext db,
        CancellationToken ct)
    {
        var items = await (
                from b in db.Wellbores
                join w in db.Wells on b.WellId equals w.Id
                join f in db.Fields on w.FieldId equals f.Id
                join o in db.Operators on f.OperatorId equals o.Id
                orderby f.Name, w.Name, b.Name
                select new WellboreListItem(
                    b.Id,
                    b.Name,
                    f.Name,
                    o.Name,
                    b.Status.ToString()))
            .ToListAsync(ct);

        return Result<IReadOnlyList<WellboreListItem>>.Success(items);
    }
}