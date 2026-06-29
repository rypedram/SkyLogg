namespace SkyLogg.Shared.Features.Logbook;

public partial class FlightSummaryDto
{
    public int TotalBlockMinutes { get; set; }

    public int TotalFlightMinutes { get; set; }

    public int TotalPicMinutes { get; set; }

    public int TotalNightMinutes { get; set; }

    public int TotalIfrMinutes { get; set; }

    public int FlightCount { get; set; }

    public List<AircraftUsageStatDto> AircraftUsage { get; set; } = [];

    public List<AirportStatDto> TopAirports { get; set; } = [];
}

public partial class AircraftUsageStatDto
{
    public Guid AircraftId { get; set; }

    public string? Registration { get; set; }

    public int BlockMinutes { get; set; }

    public int FlightCount { get; set; }
}

public partial class AirportStatDto
{
    public Guid AirportId { get; set; }

    public string? ICAO { get; set; }

    public string? Name { get; set; }

    public int VisitCount { get; set; }
}
