namespace SkyLogg.Server.Api.Features.Logbook.Import;

public interface IExternalAviationDataProvider
{
    Task<ResolvedAirportInfo?> ResolveAirportAsync(string icaoOrIata, CancellationToken cancellationToken = default);

    Task<ResolvedAircraftTypeInfo?> ResolveAircraftTypeAsync(string aircraftDescription, CancellationToken cancellationToken = default);
}
