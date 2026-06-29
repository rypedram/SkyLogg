namespace SkyLogg.Shared.Features.Logbook;

public partial class FlightLogDto : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AircraftId == Guid.Empty)
            yield return new ValidationResult(nameof(AppStrings.RequiredAttribute_ValidationError), [nameof(AircraftId)]);

        for (var i = 0; i < Sectors.Count; i++)
        {
            var sector = Sectors[i];

            if (sector.DepartureAirportId == Guid.Empty || sector.ArrivalAirportId == Guid.Empty)
            {
                yield return new ValidationResult(nameof(AppStrings.SelectAirport), []);
                continue;
            }

            if (sector.DepartureAirportId == sector.ArrivalAirportId)
                yield return new ValidationResult(nameof(AppStrings.DepartureArrivalMustDiffer), []);

            if (sector.BlockOn <= sector.BlockOff)
                yield return new ValidationResult(nameof(AppStrings.BlockOnMustBeAfterBlockOff), []);

            if (sector.Takeoff.HasValue && sector.Takeoff.Value < sector.BlockOff)
                yield return new ValidationResult(nameof(AppStrings.TakeoffMustBeAfterBlockOff), []);

            if (sector.Landing.HasValue && sector.Landing.Value > sector.BlockOn)
                yield return new ValidationResult(nameof(AppStrings.LandingMustBeBeforeBlockOn), []);

            if (sector.Takeoff.HasValue && sector.Landing.HasValue && sector.Landing.Value < sector.Takeoff.Value)
                yield return new ValidationResult(nameof(AppStrings.LandingMustBeAfterTakeoff), []);
        }

        for (var i = 0; i < Crew.Count; i++)
        {
            if (Crew[i].CrewMemberId == Guid.Empty)
                yield return new ValidationResult(nameof(AppStrings.FlightLogRequiresCrew), []);
        }
    }
}
