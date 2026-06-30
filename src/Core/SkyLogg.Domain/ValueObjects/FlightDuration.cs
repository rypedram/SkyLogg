namespace SkyLogg.Domain.ValueObjects;

public readonly record struct FlightDuration(int BlockMinutes, int FlightMinutes)
{
    public static FlightDuration Empty => new(0, 0);
}
