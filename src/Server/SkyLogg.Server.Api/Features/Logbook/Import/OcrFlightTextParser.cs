using System.Globalization;
using System.Text.RegularExpressions;

namespace SkyLogg.Server.Api.Features.Logbook.Import;

public static partial class OcrFlightTextParser
{
    public static List<FlightImportRow> Parse(string rawText)
    {
        var blocks = SplitBlocks(rawText);
        var rows = new List<FlightImportRow>();
        var lineNumber = 0;

        foreach (var block in blocks)
        {
            var row = ParseBlock(block, ++lineNumber);
            if (row is not null)
                rows.Add(row);
        }

        return rows;
    }

    private static IEnumerable<string> SplitBlocks(string rawText)
    {
        var normalized = rawText.Replace("\r\n", "\n").Trim();
        if (normalized.Length == 0)
            yield break;

        var blocks = normalized.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (blocks.Length > 1)
        {
            foreach (var block in blocks)
                yield return block;

            yield break;
        }

        var lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var current = new List<string>();

        foreach (var line in lines)
        {
            if (LooksLikeFlightHeader(line) && current.Count > 0)
            {
                yield return string.Join('\n', current);
                current.Clear();
            }

            current.Add(line);
        }

        if (current.Count > 0)
            yield return string.Join('\n', current);
    }

    private static bool LooksLikeFlightHeader(string line)
    {
        return TryParseDate(line, out _) || RoutePattern().IsMatch(line);
    }

    private static FlightImportRow? ParseBlock(string block, int lineNumber)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
            return null;

        var header = lines[0];
        var footer = lines.Length > 1 ? string.Join(' ', lines.Skip(1)) : null;

        if (!TryParseDate(header, out var flightDate))
            return null;

        if (!TryParseRoute(header, out var departure, out var arrival))
            return null;

        var aircraft = TryParseAircraft(header);
        if (string.IsNullOrWhiteSpace(aircraft))
            return null;

        TryParseTimes(footer ?? header, out var blockOff, out var takeoff, out var landing, out var blockOn);

        var remarks = ExtractRemarks(lines);
        if (string.IsNullOrWhiteSpace(remarks) && footer is not null && HasTimeMarkers(footer) is false)
            remarks = footer;

        return new FlightImportRow
        {
            LineNumber = lineNumber,
            FlightDate = flightDate,
            Aircraft = aircraft,
            DepartureIcao = departure,
            ArrivalIcao = arrival,
            BlockOff = blockOff,
            Takeoff = takeoff,
            Landing = landing,
            BlockOn = blockOn,
            Remarks = remarks
        };
    }

    private static string? ExtractRemarks(string[] lines)
    {
        foreach (var line in lines.Skip(1))
        {
            if (HasTimeMarkers(line))
                continue;

            if (string.IsNullOrWhiteSpace(line) is false)
                return line.Trim();
        }

        return null;
    }

    private static bool HasTimeMarkers(string text)
        => OffPattern().IsMatch(text) || TakeoffPattern().IsMatch(text) || LandingPattern().IsMatch(text) || OnPattern().IsMatch(text);

    private static bool TryParseDate(string text, out DateOnly date)
    {
        date = default;

        var isoMatch = IsoDatePattern().Match(text);
        if (isoMatch.Success && DateOnly.TryParse(isoMatch.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        var dmyMatch = DmyDatePattern().Match(text);
        if (dmyMatch.Success)
        {
            var day = int.Parse(dmyMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            var month = ParseMonth(dmyMatch.Groups[2].Value);
            var year = int.Parse(dmyMatch.Groups[3].Value, CultureInfo.InvariantCulture);
            if (month > 0)
            {
                date = new DateOnly(year, month, day);
                return true;
            }
        }

        return false;
    }

    private static int ParseMonth(string month) => month.ToUpperInvariant() switch
    {
        "JAN" => 1,
        "FEB" => 2,
        "MAR" => 3,
        "APR" => 4,
        "MAY" => 5,
        "JUN" => 6,
        "JUL" => 7,
        "AUG" => 8,
        "SEP" => 9,
        "OCT" => 10,
        "NOV" => 11,
        "DEC" => 12,
        _ => 0
    };

    private static bool TryParseRoute(string text, out string departure, out string arrival)
    {
        departure = string.Empty;
        arrival = string.Empty;

        var routeMatch = RoutePattern().Match(text);
        if (routeMatch.Success)
        {
            departure = NormalizeAirportCode(routeMatch.Groups[1].Value);
            arrival = NormalizeAirportCode(routeMatch.Groups[2].Value);
            return departure.Length is (3 or 4) && arrival.Length is (3 or 4);
        }

        var codes = AirportCodePattern().Matches(text)
            .Select(m => NormalizeAirportCode(m.Value))
            .Where(c => c.Length is 3 or 4)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();

        if (codes.Count == 2)
        {
            departure = codes[0];
            arrival = codes[1];
            return true;
        }

        return false;
    }

    private static string NormalizeAirportCode(string code) => code.Trim().ToUpperInvariant();

    private static string? TryParseAircraft(string text)
    {
        var registration = RegistrationPattern().Match(text);
        if (registration.Success)
            return registration.Value.ToUpperInvariant();

        var typeMatch = AircraftTypePattern().Match(text);
        if (typeMatch.Success)
            return typeMatch.Value.Trim();

        var tokens = text.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (IsoDatePattern().IsMatch(token) || DmyDatePattern().IsMatch(token))
                continue;

            if (RoutePattern().IsMatch(token) || AirportCodePattern().IsMatch(token))
                continue;

            if (TimePattern().IsMatch(token))
                continue;

            if (token.Length >= 2)
                return token;
        }

        return null;
    }

    private static void TryParseTimes(string text, out string? blockOff, out string? takeoff, out string? landing, out string? blockOn)
    {
        blockOff = MatchTime(OffPattern(), text);
        takeoff = MatchTime(TakeoffPattern(), text);
        landing = MatchTime(LandingPattern(), text);
        blockOn = MatchTime(OnPattern(), text);

        if (blockOff is null && takeoff is null && landing is null && blockOn is null)
        {
            var times = TimePattern().Matches(text).Select(m => m.Value).ToList();
            if (times.Count >= 2)
            {
                blockOff = times[0];
                blockOn = times[^1];
                if (times.Count >= 4)
                {
                    takeoff = times[1];
                    landing = times[2];
                }
            }
        }
    }

    private static string? MatchTime(Regex pattern, string text)
    {
        var match = pattern.Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex(@"\b(\d{4}-\d{2}-\d{2})\b", RegexOptions.CultureInvariant)]
    private static partial Regex IsoDatePattern();

    [GeneratedRegex(@"\b(\d{1,2})\s+(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)\s+(\d{4})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DmyDatePattern();

    [GeneratedRegex(@"\b([A-Z0-9]{3,4})\s*[-/–—>→]\s*([A-Z0-9]{3,4})\b", RegexOptions.CultureInvariant)]
    private static partial Regex RoutePattern();

    [GeneratedRegex(@"\b[A-Z]{3,4}\b", RegexOptions.CultureInvariant)]
    private static partial Regex AirportCodePattern();

    [GeneratedRegex(@"\b([A-Z]{1,2}-[A-Z0-9]{2,5}|N[0-9]{1,5}[A-Z]{0,2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RegistrationPattern();

    [GeneratedRegex(@"\b(Airbus\s+A\d{3}|Boeing\s+7\d{2}|A\d{3}|B\d{3}|ATR\s*72|ATR-?\d{2,3})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AircraftTypePattern();

    [GeneratedRegex(@"\b(?:OFF|BLOCK\s*OFF)[:\s]+(\d{1,2}:\d{2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OffPattern();

    [GeneratedRegex(@"\bT/?O[:\s]+(\d{1,2}:\d{2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TakeoffPattern();

    [GeneratedRegex(@"\bLDG[:\s]+(\d{1,2}:\d{2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LandingPattern();

    [GeneratedRegex(@"\b(?:ON|BLOCK\s*ON)[:\s]+(\d{1,2}:\d{2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OnPattern();

    [GeneratedRegex(@"\b\d{1,2}:\d{2}\b", RegexOptions.CultureInvariant)]
    private static partial Regex TimePattern();
}
