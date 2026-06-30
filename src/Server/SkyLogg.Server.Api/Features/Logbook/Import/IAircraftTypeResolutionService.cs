namespace SkyLogg.Server.Api.Features.Logbook.Import;

public interface IAircraftTypeResolutionService
{
    Task<(AircraftType AircraftType, bool WasCreated)> ResolveAircraftTypeAsync(string aircraftDescription, CancellationToken cancellationToken = default);
}
