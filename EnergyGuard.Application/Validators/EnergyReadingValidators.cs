using EnergyGuard.Application.ViewModels;
using FluentValidation;

namespace EnergyGuard.Application.Validators;

public class CreateEnergyReadingRequestValidator : AbstractValidator<CreateEnergyReadingRequest>
{
    public CreateEnergyReadingRequestValidator()
    {
        RuleFor(x => x.KilowattHour).GreaterThan(0);
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.ReadingDate).Must(d => d <= DateTime.UtcNow)
            .WithMessage("ReadingDate cannot be in the future.");
    }
}
