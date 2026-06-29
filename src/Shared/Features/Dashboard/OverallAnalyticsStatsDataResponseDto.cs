namespace SkyLogg.Shared.Features.Dashboard;

public partial class OverallAnalyticsStatsDataResponseDto
{
    public int TotalFlightLogs { get; set; }

    public int TotalAircraft { get; set; }

    public int Last30DaysFlightCount { get; set; }

    public int TotalBlockMinutes { get; set; }
}
