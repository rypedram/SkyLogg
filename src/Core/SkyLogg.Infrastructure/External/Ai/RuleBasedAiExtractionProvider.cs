using System.Text.Json;
using SkyLogg.Application.Features.Import.Dtos;
using SkyLogg.Application.Features.Import.Services;

namespace SkyLogg.Infrastructure.External.Ai;

public sealed class RuleBasedAiExtractionProvider : IAiExtractionProvider
{
    public Task<IReadOnlyList<ImportFlightCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken = default)
    {
        var lines = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.StartsWith('#') is false)
            .ToList();

        var candidates = new List<ImportFlightCandidateDto>();
        foreach (var (line, index) in lines.Select((line, index) => (line, index + 1)))
        {
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 8 || DateOnly.TryParse(parts[0], out var flightDate) is false)
                continue;

            candidates.Add(new ImportFlightCandidateDto
            {
                FlightDate = flightDate,
                AircraftType = parts[1],
                DepartureIcao = parts[2]?.ToUpperInvariant(),
                ArrivalIcao = parts[3]?.ToUpperInvariant(),
                BlockOff = parts[4],
                Takeoff = parts[5],
                Landing = parts[6],
                BlockOn = parts[7],
                Remarks = parts.Length > 8 ? string.Join(", ", parts.Skip(8)) : null,
                OverallConfidence = 0.75,
            });
        }

        return Task.FromResult<IReadOnlyList<ImportFlightCandidateDto>>(candidates);
    }

    public static string ToStructuredJson(IReadOnlyList<ImportFlightCandidateDto> flights)
        => JsonSerializer.Serialize(new { flights });
}
