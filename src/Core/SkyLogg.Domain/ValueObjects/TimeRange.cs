namespace SkyLogg.Domain.ValueObjects;

public readonly record struct TimeRange(DateTimeOffset Start, DateTimeOffset End)
{
    public int DurationMinutes
    {
        get
        {
            var minutes = (int)Math.Round((End - Start).TotalMinutes, MidpointRounding.AwayFromZero);
            return minutes < 0 ? 0 : minutes;
        }
    }

    public bool IsValid => End > Start;
}
