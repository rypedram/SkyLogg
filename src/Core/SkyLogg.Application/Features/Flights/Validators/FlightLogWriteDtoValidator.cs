using FluentValidation;
using SkyLogg.Application.Features.Flights.Dtos;

namespace SkyLogg.Application.Features.Flights.Validators;

public sealed class FlightLogWriteDtoValidator : AbstractValidator<FlightLogWriteDto>
{
    public FlightLogWriteDtoValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.FlightDate).NotEmpty();
        RuleFor(x => x.Sectors).NotEmpty();
        RuleForEach(x => x.Sectors).SetValidator(new FlightSectorDtoValidator());
    }
}

public sealed class FlightSectorDtoValidator : AbstractValidator<FlightSectorDto>
{
    public FlightSectorDtoValidator()
    {
        RuleFor(x => x.DepartureAirportId).NotEmpty();
        RuleFor(x => x.ArrivalAirportId).NotEmpty();
        RuleFor(x => x).Must(s => s.DepartureAirportId != s.ArrivalAirportId)
            .WithMessage("Departure and arrival airports must be different.");
        RuleFor(x => x.BlockOn).GreaterThan(x => x.BlockOff);
    }
}
