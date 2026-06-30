namespace SkyLogg.Server.Api.Features.Logbook.Import;

internal static class HeuristicAircraftTypeProvider
{
    public static ResolvedAircraftTypeInfo? Resolve(string aircraftDescription)
    {
        var normalized = aircraftDescription.Trim();
        var upper = normalized.ToUpperInvariant();

        ResolvedAircraftTypeInfo? result = upper switch
        {
            _ when upper.Contains("A320", StringComparison.Ordinal) => new()
            {
                Manufacturer = "Airbus",
                Model = "A320",
                TypeCode = "A320",
                Category = "Airliner",
                EngineType = "Jet",
                EngineCount = 2
            },
            _ when upper.Contains("A321", StringComparison.Ordinal) => new()
            {
                Manufacturer = "Airbus",
                Model = "A321",
                TypeCode = "A321",
                Category = "Airliner",
                EngineType = "Jet",
                EngineCount = 2
            },
            _ when upper.Contains("737", StringComparison.Ordinal) => new()
            {
                Manufacturer = "Boeing",
                Model = "737",
                TypeCode = "B737",
                Category = "Airliner",
                EngineType = "Jet",
                EngineCount = 2
            },
            _ when upper.StartsWith("MD-", StringComparison.Ordinal) => new()
            {
                Manufacturer = "McDonnell Douglas",
                Model = upper,
                TypeCode = upper,
                Category = "Airliner",
                EngineType = "Jet",
                EngineCount = 2
            },
            _ when upper.Contains("ATR", StringComparison.Ordinal) || upper.Contains("72", StringComparison.Ordinal) => new()
            {
                Manufacturer = "ATR",
                Model = "72",
                TypeCode = "AT72",
                Category = "Turboprop",
                EngineType = "Turboprop",
                EngineCount = 2
            },
            _ => null
        };

        if (result is not null)
            return result;

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            return new ResolvedAircraftTypeInfo
            {
                Manufacturer = parts[0],
                Model = string.Join(' ', parts.Skip(1)),
                TypeCode = parts[^1].ToUpperInvariant(),
                Category = "Unknown",
                EngineType = "Unknown"
            };
        }

        return new ResolvedAircraftTypeInfo
        {
            Manufacturer = "Unknown",
            Model = normalized,
            TypeCode = normalized.Length > 10 ? normalized[..10].ToUpperInvariant() : normalized.ToUpperInvariant(),
            Category = "Unknown",
            EngineType = "Unknown"
        };
    }
}
