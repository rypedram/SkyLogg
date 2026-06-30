using FluentValidation;
using SkyLogg.Application.Features.Airports.Dtos;

namespace SkyLogg.Application.Features.Airports.Validators;

public sealed class AirportWriteDtoValidator : AbstractValidator<AirportWriteDto>
{
    public AirportWriteDtoValidator()
    {
        RuleFor(x => x.Icao).NotEmpty().Length(4);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
