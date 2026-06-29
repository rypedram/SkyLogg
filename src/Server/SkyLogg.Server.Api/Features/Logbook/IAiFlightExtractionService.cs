using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public interface IAiFlightExtractionService
{
    Task<List<FlightImportCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken);
}

public sealed class RuleBasedFlightExtractionService : IAiFlightExtractionService
{
    public Task<List<FlightImportCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken)
    {
        var candidates = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseLine)
            .Where(candidate => candidate is not null)
            .Select(candidate => candidate!)
            .ToList();

        return Task.FromResult(candidates);
    }

    private static FlightImportCandidateDto? ParseLine(string line)
    {
        var parts = line.Split([',', ';', '\t'], StringSplitOptions.TrimEntries);
        if (parts.Length < 7 || DateOnly.TryParse(parts[0], out var flightDate) is false)
            return null;

        return new FlightImportCandidateDto
        {
            FlightDate = flightDate,
            AircraftRegistration = parts.ElementAtOrDefault(1),
            DepartureAirportCode = parts.ElementAtOrDefault(2),
            ArrivalAirportCode = parts.ElementAtOrDefault(3),
            BlockOff = parts.ElementAtOrDefault(4),
            Takeoff = parts.ElementAtOrDefault(5),
            Landing = parts.ElementAtOrDefault(6),
            BlockOn = parts.ElementAtOrDefault(7),
            Remarks = parts.ElementAtOrDefault(8),
            Confidence = 0.75,
        };
    }
}
