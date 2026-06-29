using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public interface IFlightTimeCalculator
{
    (int BlockMinutes, int FlightMinutes) Calculate(
        DateTimeOffset blockOff,
        DateTimeOffset blockOn,
        DateTimeOffset? takeoff,
        DateTimeOffset? landing);
}

public partial class FlightTimeCalculator : IFlightTimeCalculator
{
    public (int BlockMinutes, int FlightMinutes) Calculate(
        DateTimeOffset blockOff,
        DateTimeOffset blockOn,
        DateTimeOffset? takeoff,
        DateTimeOffset? landing)
    {
        var blockMinutes = FlightTimeFormatting.RoundNearestMinute((blockOn - blockOff).TotalMinutes);

        if (blockMinutes < 0)
            blockMinutes = 0;

        int flightMinutes;
        if (takeoff.HasValue && landing.HasValue)
        {
            flightMinutes = FlightTimeFormatting.RoundNearestMinute((landing.Value - takeoff.Value).TotalMinutes);
            if (flightMinutes < 0)
                flightMinutes = 0;
        }
        else
        {
            flightMinutes = blockMinutes;
        }

        return (blockMinutes, flightMinutes);
    }
}
