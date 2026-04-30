using NorthLog.Domain.Entities;
using NorthLog.Infrastructure.Persistence;

namespace NorthLog.Infrastructure.Seed;

public class DataSeeder(AppDbContext db) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (db.Wells.Any()) return;

        var bp = new Operator("BP");
        var asdaOil = new Operator("Asda Oil");
        var dinoJuice = new Operator("Dino Juice");
        var amberNectar = new Operator("Amber Nectar");
        db.Operators.AddRange(bp, asdaOil, dinoJuice, amberNectar);

        var forties = new Field("Forties", "21/10", bp.Id);
        var clair = new Field("Clair", "206/8", bp.Id);
        var brent = new Field("Brent", "211/29", asdaOil.Id);
        var mariner = new Field("Mariner", "9/11a", dinoJuice.Id);
        var buzzard = new Field("Buzzard", "20/6", amberNectar.Id);
        db.Fields.AddRange(forties, clair, brent, mariner, buzzard);

        var w1 = new Well("21/10-A14", forties.Id);
        var w2 = new Well("206/8-15Z", clair.Id);
        var w3 = new Well("211/29-B22", brent.Id);
        var w4 = new Well("9/11a-M07", mariner.Id);
        var w5 = new Well("20/6-K11", buzzard.Id);
        db.Wells.AddRange(w1, w2, w3, w4, w5);

        var b1 = w1.AddWellbore("21/10-A14", 0m);
        var b2 = w2.AddWellbore("206/8-15Z S1", 1500m);
        var b3 = w3.AddWellbore("211/29-B22", 0m);
        var b4 = w4.AddWellbore("9/11a-M07", 0m);
        var b5 = w5.AddWellbore("20/6-K11", 0m);
        db.Wellbores.AddRange(b1, b2, b3, b4, b5);

        b3.SubmitDailyReport(new DateOnly(2026, 4, 22), 1850m, 1980m, 9400,
            "Mostly grey gloop with the occasional sandy bit. Squidgy.",
            "Mudloggers reckon we may find oil. Coffee machine on rig is broken. SOS.");

        b3.SubmitDailyReport(new DateOnly(2026, 4, 23), 1980m, 2110m, 8700,
            "Proper sand now. ",
            "Still no oil. Coffee machine still broken which is more worrying.");

        b3.SubmitDailyReport(new DateOnly(2026, 4, 24), 2110m, 2225m, 7900,
            "Sand getting finer towards the bottom. Stuff looks like the sand trap at Trump international down south in Aberdeen.",
            "Digging deeper. Galley out of bacon rolls.");

        b3.SubmitDailyReport(new DateOnly(2026, 4, 25), 2225m, 2310m, 6200,
            "Definitely the reservoir. Faint smell of petrol.",
            "Drilling team playing Wellerman by nathan Evans already.");

        await db.SaveChangesAsync(ct);
    }
}