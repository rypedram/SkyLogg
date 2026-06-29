namespace SkyLogg.Shared.Features.Logbook;

public static class FlightTimeFormatting
{
    public static string FormatMinutes(int totalMinutes)
    {
        if (totalMinutes < 0)
            totalMinutes = 0;

        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}:{minutes:D2}";
    }

    public static int RoundNearestMinute(double totalMinutes) =>
        (int)Math.Round(totalMinutes, MidpointRounding.AwayFromZero);
}
