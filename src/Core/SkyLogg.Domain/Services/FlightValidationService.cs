using SkyLogg.Domain.Entities;
using SkyLogg.Domain.Exceptions;

namespace SkyLogg.Domain.Services;

public static class FlightValidationService
{
    public static void ValidateSector(FlightSector sector)
    {
        var errors = new List<string>();

        if (sector.DepartureAirportId == sector.ArrivalAirportId)
            errors.Add("Departure and arrival airports must be different.");

        if (sector.BlockOn <= sector.BlockOff)
            errors.Add("Block on must be after block off.");

        if (sector.Takeoff.HasValue && sector.Takeoff.Value < sector.BlockOff)
            errors.Add("Takeoff must be on or after block off.");

        if (sector.Landing.HasValue && sector.Landing.Value > sector.BlockOn)
            errors.Add("Landing must be on or before block on.");

        if (sector.FlightTimeMinutes > 24 * 60)
            errors.Add("Flight duration exceeds maximum allowed value.");

        if (errors.Count > 0)
            throw new DomainValidationException(errors);
    }

    public static void ValidateFlightLog(FlightLog flightLog, Aircraft aircraft)
    {
        if (aircraft.IsArchived)
            throw new DomainValidationException("Aircraft must be active.");

        if (flightLog.Sectors.Count == 0)
            throw new DomainValidationException("At least one flight sector is required.");

        foreach (var sector in flightLog.Sectors)
            ValidateSector(sector);
    }

    public static void ValidateRegistrationUniqueness(string registration, IEnumerable<Aircraft> existing, Guid? excludeId = null)
    {
        var normalized = registration.Trim().ToUpperInvariant();
        var duplicate = existing.Any(a =>
            a.Id != excludeId &&
            string.Equals(a.Registration, normalized, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            throw new DomainValidationException("Aircraft registration must be unique.");
    }
}
