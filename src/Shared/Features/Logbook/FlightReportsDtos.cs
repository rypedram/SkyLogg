namespace SkyLogg.Shared.Features.Logbook;

public partial class FlightStatisticsDto
{
    public int TotalFlightMinutes { get; set; }

    public int TotalNightMinutes { get; set; }

    public int TotalIfrMinutes { get; set; }

    public int FlightCount { get; set; }

    public int AircraftCount { get; set; }

    public int AirportCount { get; set; }

    public FlightStatisticItemDto? LongestFlight { get; set; }

    public FlightStatisticItemDto? ShortestFlight { get; set; }

    public string? MostUsedAircraft { get; set; }

    public string? MostVisitedAirport { get; set; }

    public List<MonthlyFlightTrendDto> MonthlyTrends { get; set; } = [];
}

public partial class FlightStatisticItemDto
{
    public Guid FlightLogId { get; set; }

    public DateOnly FlightDate { get; set; }

    public string? Route { get; set; }

    public int FlightMinutes { get; set; }
}

public partial class MonthlyFlightTrendDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public int FlightMinutes { get; set; }

    public int FlightCount { get; set; }
}

public partial class AchievementStatusDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int Threshold { get; set; }

    public int CurrentValue { get; set; }

    public bool IsUnlocked { get; set; }

    public DateTimeOffset? UnlockedOn { get; set; }
}
