namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IFlightImportController : IAppController
{
    [HttpPost]
    Task<FlightImportPreviewDto> Preview(FlightImportRequestDto request, CancellationToken cancellationToken);

    [HttpPost]
    Task<FlightImportConfirmResultDto> Confirm(FlightImportConfirmDto request, CancellationToken cancellationToken);
}
