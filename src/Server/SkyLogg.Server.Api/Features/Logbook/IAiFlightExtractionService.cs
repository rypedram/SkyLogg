using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public interface IAiFlightExtractionService
{
    Task<List<FlightImportCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken);
}

public sealed partial class RuleBasedFlightExtractionService : IAiFlightExtractionService
{
    public Task<List<FlightImportCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken)
    {
        var candidates = Import.FlightImportTextExtractor.ExtractRows(rawText)
            .Select(row => new FlightImportCandidateDto
            {
                LineNumber = row.LineNumber,
                FlightDate = row.FlightDate,
                AircraftType = row.Aircraft,
                DepartureAirportCode = row.DepartureIcao,
                ArrivalAirportCode = row.ArrivalIcao,
                BlockOff = row.BlockOff,
                Takeoff = row.Takeoff,
                Landing = row.Landing,
                BlockOn = row.BlockOn,
                Remarks = row.Remarks,
                Confidence = 0.75,
            })
            .ToList();

        return Task.FromResult(candidates);
    }
}
