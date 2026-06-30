using FluentValidation;

namespace SkyLogg.Shared.Features.Logbook;

public class FlightSectorDtoValidator : AbstractValidator<FlightSectorDto>
{
    public FlightSectorDtoValidator()
    {
        RuleFor(s => s.DepartureAirportId).NotEmpty();
        RuleFor(s => s.ArrivalAirportId).NotEmpty();
        RuleFor(s => s).Must(s => s.DepartureAirportId != s.ArrivalAirportId)
            .WithMessage(nameof(AppStrings.DepartureArrivalMustDiffer));
        RuleFor(s => s.BlockOn).GreaterThan(s => s.BlockOff)
            .WithMessage(nameof(AppStrings.BlockOnMustBeAfterBlockOff));
        RuleFor(s => s.Takeoff).GreaterThanOrEqualTo(s => s.BlockOff)
            .When(s => s.Takeoff.HasValue)
            .WithMessage(nameof(AppStrings.TakeoffMustBeAfterBlockOff));
        RuleFor(s => s.Landing).LessThanOrEqualTo(s => s.BlockOn)
            .When(s => s.Landing.HasValue)
            .WithMessage(nameof(AppStrings.LandingMustBeBeforeBlockOn));
        RuleFor(s => s.Landing).GreaterThanOrEqualTo(s => s.Takeoff)
            .When(s => s.Takeoff.HasValue && s.Landing.HasValue)
            .WithMessage(nameof(AppStrings.LandingMustBeAfterTakeoff));
        RuleFor(s => s.DayTakeoffs).GreaterThanOrEqualTo(0);
        RuleFor(s => s.NightTakeoffs).GreaterThanOrEqualTo(0);
        RuleFor(s => s.DayLandings).GreaterThanOrEqualTo(0);
        RuleFor(s => s.NightLandings).GreaterThanOrEqualTo(0);
        RuleFor(s => s.PicTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(s => s.SicTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(s => s.DualTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(s => s.NightTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(s => s.IfrTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(s => s)
            .Must(s => s.PicTimeMinutes + s.SicTimeMinutes + s.DualTimeMinutes <= s.FlightTimeMinutes)
            .When(s => s.FlightTimeMinutes > 0)
            .WithMessage(nameof(AppStrings.FlightRoleTimesMustFitFlightTime));
        RuleFor(s => s.NightTimeMinutes).LessThanOrEqualTo(s => s.FlightTimeMinutes)
            .When(s => s.FlightTimeMinutes > 0)
            .WithMessage(nameof(AppStrings.NightTimeMustFitFlightTime));
        RuleFor(s => s.IfrTimeMinutes).LessThanOrEqualTo(s => s.FlightTimeMinutes)
            .When(s => s.FlightTimeMinutes > 0)
            .WithMessage(nameof(AppStrings.IfrTimeMustFitFlightTime));
    }
}

public class FlightLogDtoValidator : AbstractValidator<FlightLogDto>
{
    public FlightLogDtoValidator()
    {
        RuleFor(f => f.FlightDate).NotEmpty();
        RuleFor(f => f.AircraftId).NotEmpty();
        RuleFor(f => f.Sectors).NotEmpty().WithMessage(nameof(AppStrings.FlightLogRequiresSector));
        RuleFor(f => f.Crew).NotEmpty().WithMessage(nameof(AppStrings.FlightLogRequiresCrew));
        RuleForEach(f => f.Sectors).SetValidator(new FlightSectorDtoValidator());
        RuleForEach(f => f.Crew).ChildRules(crew =>
        {
            crew.RuleFor(c => c.CrewMemberId).NotEmpty();
        });
        RuleFor(f => f.Crew)
            .Must(crew => crew.Select(c => c.CrewMemberId).Distinct().Count() == crew.Count)
            .WithMessage(nameof(AppStrings.DuplicateCrewMemberOnFlight));
        RuleFor(f => f.Remarks).MaximumLength(2000);
    }
}
