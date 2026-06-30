using SkyLogg.Server.Api.Features.Logbook.Import.OurAirports;

namespace SkyLogg.Server.Api.Features.Logbook.Import;

public sealed partial class OurAirportsAviationDataProvider : IExternalAviationDataProvider
{
    [AutoInject] private OurAirportsCatalog catalog = default!;

    public async Task<ResolvedAirportInfo?> ResolveAirportAsync(string icaoOrIata, CancellationToken cancellationToken = default)
    {
        var airport = await catalog.LookupAsync(icaoOrIata, cancellationToken);
        if (airport is null)
            return null;

        string? timeZoneIanaId = null;
        try
        {
            timeZoneIanaId = global::GeoTimeZone.TimeZoneLookup.GetTimeZone(airport.Latitude, airport.Longitude).Result;
        }
        catch
        {
            // Time zone lookup is best-effort; UTC fallback is applied downstream.
        }

        return new ResolvedAirportInfo
        {
            Icao = airport.Icao,
            Iata = airport.Iata,
            Name = airport.Name,
            City = string.IsNullOrWhiteSpace(airport.City) ? airport.CountryName : airport.City,
            Country = airport.CountryName,
            CountryIso2 = airport.CountryIso2,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            TimeZoneIanaId = timeZoneIanaId,
            ElevationFt = airport.ElevationFt
        };
    }

    public Task<ResolvedAircraftTypeInfo?> ResolveAircraftTypeAsync(string aircraftDescription, CancellationToken cancellationToken = default)
        => Task.FromResult<ResolvedAircraftTypeInfo?>(HeuristicAircraftTypeProvider.Resolve(aircraftDescription));
}
