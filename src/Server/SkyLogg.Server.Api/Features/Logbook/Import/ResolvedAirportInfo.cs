namespace SkyLogg.Server.Api.Features.Logbook.Import;

public sealed class ResolvedAirportInfo
{
    public required string Icao { get; init; }

    public string? Iata { get; init; }

    public required string Name { get; init; }

    public required string City { get; init; }

    public required string Country { get; init; }

    public string? CountryIso2 { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string? TimeZoneIanaId { get; init; }

    public int? ElevationFt { get; init; }
}
