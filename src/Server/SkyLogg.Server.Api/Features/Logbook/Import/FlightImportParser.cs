namespace SkyLogg.Server.Api.Features.Logbook.Import;

public sealed class FlightImportRow
{
    public int LineNumber { get; init; }

    public DateOnly? FlightDate { get; init; }

    public string? Aircraft { get; init; }

    public string? DepartureIcao { get; init; }

    public string? ArrivalIcao { get; init; }

    public string? BlockOff { get; init; }

    public string? Takeoff { get; init; }

    public string? Landing { get; init; }

    public string? BlockOn { get; init; }

    public string? Remarks { get; init; }
}

public static class FlightImportParser
{
    public static List<FlightImportRow> Parse(string rawText)
    {
        return rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((line, index) => ParseLine(line, index + 1))
            .Where(row => row is not null)
            .Select(row => row!)
            .ToList();
    }

    private static FlightImportRow? ParseLine(string line, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            return null;

        var parts = line.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length < 8)
            return null;

        if (!DateOnly.TryParse(parts[0], out var flightDate))
            return null;

        return new FlightImportRow
        {
            LineNumber = lineNumber,
            FlightDate = flightDate,
            Aircraft = parts[1],
            DepartureIcao = parts[2]?.ToUpperInvariant(),
            ArrivalIcao = parts[3]?.ToUpperInvariant(),
            BlockOff = parts[4],
            Takeoff = parts[5],
            Landing = parts[6],
            BlockOn = parts[7],
            Remarks = parts.Length > 8 ? string.Join(", ", parts.Skip(8)) : null
        };
    }
}

public static class FlightImportTimeHelper
{
    public static DateTimeOffset? Combine(DateOnly flightDate, string? time)
    {
        if (string.IsNullOrWhiteSpace(time))
            return null;

        if (!TimeOnly.TryParse(time, out var parsed))
            return null;

        return new DateTimeOffset(flightDate.ToDateTime(parsed), TimeSpan.Zero);
    }
}
