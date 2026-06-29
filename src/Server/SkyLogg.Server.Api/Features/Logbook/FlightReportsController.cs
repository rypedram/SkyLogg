using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightReportsController : AppControllerBase, IFlightReportsController
{
    [AutoInject] private FlightReportsService flightReportsService = default!;

    [HttpGet]
    public async Task<FlightStatisticsDto> GetStatistics(CancellationToken cancellationToken)
    {
        return await flightReportsService.GetStatisticsAsync(User.GetUserId(), cancellationToken);
    }

    [HttpGet]
    public async Task<List<AchievementStatusDto>> GetAchievements(CancellationToken cancellationToken)
    {
        return await flightReportsService.GetAchievementsAsync(User.GetUserId(), cancellationToken);
    }
}
