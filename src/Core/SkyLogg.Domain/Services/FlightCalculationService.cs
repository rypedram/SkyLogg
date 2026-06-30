using SkyLogg.Domain.Entities;
using SkyLogg.Domain.ValueObjects;

namespace SkyLogg.Domain.Services;

public static class FlightCalculationService
{
    public static FlightDuration CalculateSectorDuration(
        DateTimeOffset blockOff,
        DateTimeOffset blockOn,
        DateTimeOffset? takeoff,
        DateTimeOffset? landing)
    {
        var blockRange = new TimeRange(blockOff, blockOn);
        var blockMinutes = blockRange.DurationMinutes;

        int flightMinutes;
        if (takeoff.HasValue && landing.HasValue)
        {
            flightMinutes = new TimeRange(takeoff.Value, landing.Value).DurationMinutes;
        }
        else
        {
            flightMinutes = blockMinutes;
        }

        return new FlightDuration(blockMinutes, flightMinutes);
    }

    public static void ApplyTotals(FlightLog flightLog)
    {
        flightLog.TotalBlockMinutes = flightLog.Sectors.Sum(s => s.BlockTimeMinutes);
        flightLog.TotalFlightMinutes = flightLog.Sectors.Sum(s => s.FlightTimeMinutes);
        flightLog.TotalPicMinutes = flightLog.Sectors.Sum(s => s.PicTimeMinutes);
        flightLog.TotalSicMinutes = flightLog.Sectors.Sum(s => s.SicTimeMinutes);
        flightLog.TotalDualMinutes = flightLog.Sectors.Sum(s => s.DualTimeMinutes);
        flightLog.TotalNightMinutes = flightLog.Sectors.Sum(s => s.NightTimeMinutes);
        flightLog.TotalIfrMinutes = flightLog.Sectors.Sum(s => s.IfrTimeMinutes);
        flightLog.TotalLandings = flightLog.Sectors.Sum(s => s.DayLandings + s.NightLandings);
    }
}
