namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IFlightReportsController : IAppController
{
    [HttpGet]
    Task<FlightStatisticsDto> GetStatistics(CancellationToken cancellationToken);

    [HttpGet]
    Task<List<AchievementStatusDto>> GetAchievements(CancellationToken cancellationToken);
}
