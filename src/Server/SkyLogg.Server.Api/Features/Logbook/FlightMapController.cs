using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightMapController : AppControllerBase, IFlightMapController
{
    [AutoInject] private FlightMapService flightMapService = default!;

    [HttpGet]
    public async Task<FlightMapDto> GetMapData(DateOnly? fromDate, DateOnly? toDate, Guid? aircraftId, int? year, int? month, CancellationToken cancellationToken)
    {
        return await flightMapService.GetMapDataAsync(User.GetUserId(), fromDate, toDate, aircraftId, year, month, cancellationToken);
    }
}
