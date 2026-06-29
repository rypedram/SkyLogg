namespace SkyLogg.Shared.Features.Logbook;

public partial class FlightMapDto
{
    public string ProviderKey { get; set; } = "svg";

    public int FlightCount { get; set; }

    public int TotalFlightMinutes { get; set; }

    public List<FlightMapRouteDto> Routes { get; set; } = [];

    public List<FlightMapAirportPinDto> Airports { get; set; } = [];

    public List<FlightMapCountryStatDto> CountryStats { get; set; } = [];
}

public partial class FlightMapRouteDto
{
    public Guid FlightLogId { get; set; }

    public DateOnly FlightDate { get; set; }

    public string? AircraftRegistration { get; set; }

    public string? DepartureAirport { get; set; }

    public string? ArrivalAirport { get; set; }

    public double DepartureLatitude { get; set; }

    public double DepartureLongitude { get; set; }

    public double ArrivalLatitude { get; set; }

    public double ArrivalLongitude { get; set; }

    public int FlightTimeMinutes { get; set; }

    public List<FlightMapPointDto> GreatCirclePoints { get; set; } = [];
}

public partial class FlightMapPointDto
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

public partial class FlightMapAirportPinDto
{
    public Guid AirportId { get; set; }

    public string? ICAO { get; set; }

    public string? IATA { get; set; }

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int VisitCount { get; set; }
}

public partial class FlightMapCountryStatDto
{
    public string? Country { get; set; }

    public int VisitCount { get; set; }
}
