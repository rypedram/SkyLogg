namespace SkyLogg.Application.Features.Map.Dtos;

public sealed class FlightMapRouteDto
{
    public Guid FlightLogId { get; init; }

    public Guid SectorId { get; init; }

    public string DepartureIcao { get; init; } = string.Empty;

    public string ArrivalIcao { get; init; } = string.Empty;

    public double DepartureLatitude { get; init; }

    public double DepartureLongitude { get; init; }

    public double ArrivalLatitude { get; init; }

    public double ArrivalLongitude { get; init; }

    public DateOnly FlightDate { get; init; }

    public Guid AircraftId { get; init; }
}

public sealed class FlightMapAirportPinDto
{
    public Guid AirportId { get; init; }

    public string Icao { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public int VisitCount { get; init; }

    public DateOnly? LastVisited { get; init; }
}

public sealed class FlightMapDataDto
{
    public List<FlightMapRouteDto> Routes { get; init; } = [];

    public List<FlightMapAirportPinDto> Airports { get; init; } = [];
}
