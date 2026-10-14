using EnergyGuard.Application.ViewModels;
using FluentValidation;

namespace EnergyGuard.Application.Validators;

public class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SerialNumber).NotEmpty();
        RuleFor(x => x.PowerLimitKwh).GreaterThan(0);
    }
}
