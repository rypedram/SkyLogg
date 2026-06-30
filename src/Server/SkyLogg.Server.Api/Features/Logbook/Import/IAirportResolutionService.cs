namespace SkyLogg.Server.Api.Features.Logbook.Import;

public interface IAirportResolutionService
{
    Task<(Airport Airport, bool WasCreated)> GetOrCreateAirportByCodeAsync(string airportCode, CancellationToken cancellationToken = default);
}
