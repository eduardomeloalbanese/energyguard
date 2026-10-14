using EnergyGuard.Application.ViewModels;
using FluentValidation;

namespace EnergyGuard.Application.Validators;

public class GenerateReportRequestValidator : AbstractValidator<GenerateReportRequest>
{
    public GenerateReportRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.PeriodEnd).GreaterThan(x => x.PeriodStart)
            .WithMessage("PeriodEnd must be greater than PeriodStart.");
    }
}
