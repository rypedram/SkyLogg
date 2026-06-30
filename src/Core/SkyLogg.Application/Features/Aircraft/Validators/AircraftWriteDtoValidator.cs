using FluentValidation;
using SkyLogg.Application.Features.Aircraft.Dtos;

namespace SkyLogg.Application.Features.Aircraft.Validators;

public sealed class AircraftWriteDtoValidator : AbstractValidator<AircraftWriteDto>
{
    public AircraftWriteDtoValidator()
    {
        RuleFor(x => x.Registration).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
    }
}
