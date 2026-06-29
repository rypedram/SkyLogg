namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IFlightMapController : IAppController
{
    [HttpGet]
    Task<FlightMapDto> GetMapData(DateOnly? fromDate, DateOnly? toDate, Guid? aircraftId, int? year, int? month, CancellationToken cancellationToken);
}
