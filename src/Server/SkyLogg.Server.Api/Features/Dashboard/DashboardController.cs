using SkyLogg.Shared.Features.Dashboard;

namespace SkyLogg.Server.Api.Features.Dashboard;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]"),
    Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS),
    Authorize(Policy = AppFeatures.AdminPanel.Dashboard)]
public partial class DashboardController : AppControllerBase, IDashboardController
{
    [HttpGet]
    public async Task<OverallAnalyticsStatsDataResponseDto> GetOverallAnalyticsStatsData(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var last30Days = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        return new OverallAnalyticsStatsDataResponseDto
        {
            TotalFlightLogs = await DbContext.FlightLogs.CountAsync(f => f.UserId == userId && !f.Deleted, cancellationToken),
            TotalAircraft = await DbContext.Aircrafts.CountAsync(a => a.IsActive, cancellationToken),
            Last30DaysFlightCount = await DbContext.FlightLogs.CountAsync(f => f.UserId == userId && !f.Deleted && f.FlightDate >= last30Days, cancellationToken),
            TotalBlockMinutes = await DbContext.FlightLogs.Where(f => f.UserId == userId && !f.Deleted).SumAsync(f => f.TotalBlockMinutes, cancellationToken),
        };
    }
}
