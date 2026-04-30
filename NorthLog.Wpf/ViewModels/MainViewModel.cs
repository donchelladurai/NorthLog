using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NorthLog.Application.Common;
using NorthLog.Application.Featrures.Wellbores;
using NorthLog.Application.Features.DailyReports.SubmitDailyReport;
using NorthLog.Application.Features.Wellbores.GetWellboreDetails;
using NorthLog.Application.Features.Wellbores.ListWellbores;
using NorthLog.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;
using Wolverine;

namespace NorthLog.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopes;

    public ObservableCollection<WellboreListItem> Wellbores { get; } = [];
    public ObservableCollection<DailyReportRow> Reports { get; } = [];

    public MainViewModel(IServiceScopeFactory scopes)
    {
        _scopes = scopes;

        LoadWellboresCommand = new AsyncRelayCommand(LoadWellboresAsync);
        SubmitReportCommand = new AsyncRelayCommand(SubmitReportAsync, CanSubmit);
    }

    // ----- Wellbore selection -----

    private WellboreListItem? _selectedWellbore;
    public WellboreListItem? SelectedWellbore
    {
        get => _selectedWellbore;
        set
        {
            if (Set(ref _selectedWellbore, value))
            {
                _ = LoadDetailsAsync();
                SubmitReportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    // Read only

    private string? _wellName;
    public string? WellName { get => _wellName; private set => Set(ref _wellName, value); }

    private string? _fieldName;
    public string? FieldName { get => _fieldName; private set => Set(ref _fieldName, value); }

    private string? _operatorName;
    public string? OperatorName { get => _operatorName; private set => Set(ref _operatorName, value); }

    private string? _status;
    public string? Status { get => _status; private set => Set(ref _status, value); }

    private decimal _kickoffDepth;
    public decimal KickoffDepth { get => _kickoffDepth; private set => Set(ref _kickoffDepth, value); }

    // Submit-report form (string-bound to dodge culture/decimal pain)

    private DateTime _reportDate = DateTime.Today;
    public DateTime ReportDate { get => _reportDate; set => Set(ref _reportDate, value); }

    private string _depthIn = "";
    public string DepthIn { get => _depthIn; set => Set(ref _depthIn, value); }

    private string _depthOut = "";
    public string DepthOut { get => _depthOut; set => Set(ref _depthOut, value); }

    private string _totalOil = "";
    public string TotalOil { get => _totalOil; set => Set(ref _totalOil, value); }

    private string _lithology = "";
    public string Lithology { get => _lithology; set => Set(ref _lithology, value); }

    private string? _notes;
    public string? Notes { get => _notes; set => Set(ref _notes, value); }

    // Status feedback

    private string? _statusMessage;
    public string? StatusMessage { get => _statusMessage; private set => Set(ref _statusMessage, value); }

    private bool _isError;
    public bool IsError { get => _isError; private set => Set(ref _isError, value); }

    // Commands

    public AsyncRelayCommand LoadWellboresCommand { get; }
    public AsyncRelayCommand SubmitReportCommand { get; }

    private bool CanSubmit() => SelectedWellbore is not null;

    private async Task LoadWellboresAsync()
    {
        await using var scope = _scopes.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var result = await bus.InvokeAsync<Result<IReadOnlyList<WellboreListItem>>>(
            new ListWellboresQuery());

        if (result.IsFailure)
        {
            // For the wellbore list there's no realistic failure path, but I'm
            // branching on it anyway — the ViewModel only needs one branching
            // shape across handlers that return Result<T>.
            SetError(result.Error!);
            return;
        }

        Wellbores.Clear();
        foreach (var item in result.Value!) Wellbores.Add(item);

        if (Wellbores.Count > 0 && SelectedWellbore is null)
            SelectedWellbore = Wellbores[0];
    }

    private async Task LoadDetailsAsync()
    {
        Reports.Clear();
        if (SelectedWellbore is null)
        {
            WellName = FieldName = OperatorName = Status = null;
            KickoffDepth = 0;
            return;
        }

        await using var scope = _scopes.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        /* To the interviewers:
         This method has to handle GetWellboreDetailsHandler differently
         from its sibling handlers. Because that handler throws on not-found
         (deliberately bad — see the comment block in
         GetWellboreDetailsHandler.cs), we need a try/catch here. The other
         two Result<T> returning handlers are handled with a clean
         `if (result.IsFailure)` branch.
        
         This is exactly the cost the bad foil's design imposes on every
         caller: an inconsistent error shape across handlers means more
         branching code and a higher chance the catch is forgotten and the app crashes. */

        try
        {
            var details = await bus.InvokeAsync<WellboreDetails>(
                new GetWellboreDetailsQuery(SelectedWellbore.Id));

            WellName = details.WellName;
            FieldName = details.FieldName;
            OperatorName = details.OperatorName;
            Status = details.Status;
            KickoffDepth = details.KickoffDepthMeters;

            foreach (var r in details.Reports) Reports.Add(r);
        }
        catch (WellboreNotFoundException)
        {
            WellName = FieldName = OperatorName = Status = null;
            KickoffDepth = 0;
        }
    }

    private async Task SubmitReportAsync()
    {
        StatusMessage = null;
        IsError = false;

        if (SelectedWellbore is null)
        {
            SetError("Select a wellbore first.");
            return;
        }

        if (!TryParseForm(out var cmd, out var parseError))
        {
            SetError(parseError!);
            return;
        }

        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var result = await bus.InvokeAsync<Result<Guid>>(cmd);

            if (result.IsFailure)
            {
                /* The handler converts both not-found ("Wellbore X not found")
                 and domain-invariant violations into Result.Failure. The ViewModel surfaces the
                 message as-is — In production, I would probably distinguish
                 the cases by sniffing the message or by upgrading Result<T>
                 to carry an error code. */

                SetError(result.Error!);
                return;
            }

            // result.Value is the Guid of the new report — discarded here, but
            // a more sophisticated UI might navigate to it.

            _ = result.Value;

            StatusMessage = "Report submitted.";
            IsError = false;

            ClearForm();
            await LoadDetailsAsync();
        }
        catch (ValidationException vex)
        {
            /*   Wolverine's FluentValidation middleware throws this before the
                 handler runs. The handler never sees it, it just bubbles up here.

                 I wanted to highlight the "correct" use of an exception here — a contract
                 violation by the caller, not an expected miss. */

            SetError(string.Join(Environment.NewLine, vex.Errors.Select(e => e.ErrorMessage)));
        }
    }

    private bool TryParseForm(out SubmitDailyReportCommand cmd, out string? error)
    {
        cmd = null!;
        var ci = CultureInfo.InvariantCulture;

        if (!decimal.TryParse(DepthIn, NumberStyles.Number, ci, out var di) ||
            !decimal.TryParse(DepthOut, NumberStyles.Number, ci, out var dout) ||
            !decimal.TryParse(TotalOil, NumberStyles.Number, ci, out var oil))
        {
            error = "All numeric fields must be valid decimals (use '.' as decimal separator).";
            return false;
        }

        cmd = new SubmitDailyReportCommand(
            WellboreId: SelectedWellbore!.Id,
            ReportDate: DateOnly.FromDateTime(ReportDate),
            DepthIn: di,
            DepthOut: dout,
            TotalOilInBarrels: oil,
            LithologySummary: Lithology,
            Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes);

        error = null;
        return true;
    }

    private void SetError(string message)
    {
        StatusMessage = message;
        IsError = true;
    }

    private void ClearForm()
    {
        DepthIn = DepthOut = TotalOil = Lithology = "";
        Notes = null;
        ReportDate = DateTime.Today;
    }
}