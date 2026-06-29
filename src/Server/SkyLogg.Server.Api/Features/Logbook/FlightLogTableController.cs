namespace SkyLogg.Server.Api.Features.Logbook;

/// <summary>
/// Offline sync stub — Datasync integration will be enabled in a future module.
/// </summary>
[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/FlightLogTable")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightLogTableController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, "Offline sync is not yet implemented.");
}
