using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NorthLog.Domain.Entities;
using NorthLog.Infrastructure.Persistence;
using NorthLog.Wpf.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NorthLog.Wpf.Views;

public partial class LegacyReportsWindow : Window
{
    /* To the interviewers:

     (1) The window news up its own DbContext directly. This bypasses DI,
         bypasses the IAppDbContext abstraction in the Application layer,
         bypasses Wolverine's dispatcher entirely, and means the window can
         issue arbitrary EF queries — the persistence layer has effectively
         leaked all the way to the UI.
    
     (2) The DbContext is held for the entire lifetime of the window. Its
         change tracker accumulates entities forever, memory grows, and any
         concurrent async operation can corrupt it. */

    private readonly AppDbContext _db;

    public LegacyReportsWindow()
    {
        InitializeComponent();

        var app = (App)System.Windows.Application.Current;
        var root = app.Host!.Services.GetRequiredService<InMemoryDatabaseRoot>();

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("NorthLog", root)
            .Options;
        _db = new AppDbContext(opts);

        /* To the interviewers:
             Sync-over-async on the UI thread, in the constructor!! On any real
             network database this would probably freeze the window before it had even
             rendered. */

        var bores = _db.Wellbores.ToList();
        cmbWellbore.ItemsSource = bores;
        cmbWellbore.DisplayMemberPath = "Name";

        if (bores.Count > 0) cmbWellbore.SelectedIndex = 0;

        /*   Static singleton state. AppContext.LastOpenedWellboreId is set
             here and read who-knows-where. Untestable and racy. */

        AppContext.LastOpenedWellboreId = (cmbWellbore.SelectedItem as Wellbore)?.Id;
    }

    private void cmbWellbore_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        /* To the interviewers:
                Classic N+1. We have the selected wellbore in memory but we go
                back to the database in a loop to fetch its reports. With 1000
                wellbores in the dropdown and someone clicking through them this
                would be murder on a real DB. */
        var bore = cmbWellbore.SelectedItem as Wellbore;
        if (bore == null) return;

        var rows = new List<object>();
        foreach (var b in _db.Wellbores.ToList())
        {
            var reports = _db.DailyReports
                .Where(r => r.WellboreId == b.Id)
                .ToList();
            if (b.Id == bore.Id)
            {
                foreach (var r in reports)
                {
                    rows.Add(new
                    {
                        r.ReportDate,
                        r.DepthIn,
                        r.DepthOut,
                        r.LithologySummary,
                        r.Notes
                    });
                }
            }
        }

        grid.ItemsSource = rows;
        AppContext.LastOpenedWellboreId = bore.Id;
    }

    /* To the interviewers:
        This is a god method. It does parsing, validation, business rules,
         persistence, error handling and UI updates, all in 80+ lines. There
         is no seam to test, no seam to reuse, and every change to any of
         those concerns means editing this same method.
    
         Magic strings for status comparisons, raw exception messages shown
         to the user, and a `try { ... } catch (Exception) { swallow }`
         block that hides bugs.
    
      `_db.SaveChanges()` is sync — UI thread blocks on the DB.
    
     For the contrast: see Application/Features/DailyReports/SubmitDailyReport.cs
     and ViewModels/MainViewModel.SubmitReportAsync. Same logical operation,
     single-responsibility methods, async all the way down, validation in one
     layer, invariants in another. 
    
     */
    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var bore = cmbWellbore.SelectedItem as Wellbore;
            if (bore == null)
            {
                txtStatus.Text = "no wellbore selected";
                return;
            }

            // Magic-string status comparison. Should be the WellboreStatus enum.
            if (bore.Status.ToString() == "Completed")
            {
                txtStatus.Text = "wellbore is closed";
                return;
            }

            // Manual parsing with no culture, no error message specificity.
            //      A user typing "1,850" instead of "1850" gets a useless error.
            var date = DateOnly.Parse(txtDate.Text);
            var depthIn = decimal.Parse(txtDepthIn.Text);
            var depthOut = decimal.Parse(txtDepthOut.Text);
            var oil = decimal.Parse(txtOil.Text);
            var lith = txtLithology.Text;
            var notes = txtNotes.Text;

            // Validation duplicated from FluentValidation. The two will
            //      drift apart over time and one of them will be wrong.
            if (depthIn < 0 || depthOut < 0)
            {
                txtStatus.Text = "depths must be >= 0";
                return;
            }
            if (depthOut < depthIn)
            {
                txtStatus.Text = "depth out < depth in";
                return;
            }

            // Reaching into the FormationHelper static for hardcoded data
            //      keyed by a magic string. We're appending it to the lithology
            //      summary because the legacy form has no field for it.
            var fieldKey = bore.Name.StartsWith("21/10") ? "FORTIES"
                          : bore.Name.StartsWith("211/29") ? "BRENT"
                          : "";
            var tops = FormationHelper.GetTops(fieldKey);
            var topsText = string.Join(", ", tops.Select(t => $"{t.Name} @ {t.Depth}m"));
            if (!string.IsNullOrEmpty(topsText))
                lith = lith + " | " + topsText;

            /* Mutating a domain entity by directly newing-up a DailyReport
                  and adding it to the context, bypassing the aggregate's
                  SubmitDailyReport method (and therefore its invariants). */

            var report = (DailyReport)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(DailyReport));
            _db.Entry(report).Property("WellboreId").CurrentValue = bore.Id;
            _db.Entry(report).Property("ReportDate").CurrentValue = date;
            _db.Entry(report).Property("DepthIn").CurrentValue = depthIn;
            _db.Entry(report).Property("DepthOut").CurrentValue = depthOut;
            _db.Entry(report).Property("TotalOilInBarrels").CurrentValue = oil;
            _db.Entry(report).Property("LithologySummary").CurrentValue = lith;
            _db.Entry(report).Property("Notes").CurrentValue = notes;
            _db.DailyReports.Add(report);

            _db.SaveChanges(); // sync save on UI thread

            txtStatus.Foreground = System.Windows.Media.Brushes.Green;
            txtStatus.Text = "saved";

            cmbWellbore_SelectionChanged(cmbWellbore, null!);
        }
        catch (System.Exception ex)
        {
            //     Catch-all that hides every bug, paired with a MessageBox
            //      that shows the entire stack trace to the user.
            AppContext.CounterOfThingsThatGoneWrong++;
            MessageBox.Show(ex.ToString(), "ERROR");
            txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            txtStatus.Text = ex.Message;
        }
    }
}