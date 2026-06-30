namespace SkyLogg.Server.Api.Features.Logbook.Import.OurAirports;

public sealed class OurAirportRecord
{
    public required string Icao { get; init; }

    public string? Iata { get; init; }

    public required string Name { get; init; }

    public required string City { get; init; }

    public required string CountryIso2 { get; init; }

    public required string CountryName { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public int? ElevationFt { get; init; }

    public int TypeRank { get; init; }
}
