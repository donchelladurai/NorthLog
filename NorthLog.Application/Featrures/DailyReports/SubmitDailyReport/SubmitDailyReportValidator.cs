using FluentValidation;

namespace NorthLog.Application.Features.DailyReports.SubmitDailyReport;

public class SubmitDailyReportValidator : AbstractValidator<SubmitDailyReportCommand>
{
    public SubmitDailyReportValidator()
    {
        RuleFor(x => x.WellboreId).NotEmpty();
        RuleFor(x => x.ReportDate).NotEmpty();
        RuleFor(x => x.DepthIn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DepthOut).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalOilInBarrels).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LithologySummary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}