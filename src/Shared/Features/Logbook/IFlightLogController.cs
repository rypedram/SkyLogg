namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IFlightLogController : IAppController
{
    [HttpGet]
    Task<PagedResponse<FlightLogDto>> GetFlightLogs(CancellationToken cancellationToken) => default!;

    [HttpGet("{id}")]
    Task<FlightLogDto> Get(Guid id, CancellationToken cancellationToken);

    [HttpGet]
    Task<CurrencyStatusDto> GetCurrencyStatus(CancellationToken cancellationToken);

    [HttpGet]
    Task<FlightSummaryDto> GetSummary(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    [HttpGet]
    Task<byte[]> ExportCsv(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    [HttpGet]
    Task<byte[]> ExportPdf(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    [HttpPost]
    Task<FlightLogDto> Create(FlightLogDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<FlightLogDto> Update(FlightLogDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}/{version}")]
    Task Delete(Guid id, long version, CancellationToken cancellationToken);
}
