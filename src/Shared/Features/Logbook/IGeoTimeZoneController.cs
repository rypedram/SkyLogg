namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IGeoTimeZoneController : IAppController
{
    [HttpGet]
    Task<PagedResponse<GeoTimeZoneDto>> GetTimeZones(CancellationToken cancellationToken);

    [HttpGet("{searchQuery}")]
    Task<PagedResponse<GeoTimeZoneDto>> SearchTimeZones(string searchQuery, CancellationToken cancellationToken);

    [HttpGet("{id}")]
    Task<GeoTimeZoneDto> GetById(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<GeoTimeZoneDto> Create(GeoTimeZoneDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<GeoTimeZoneDto> Update(GeoTimeZoneDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}")]
    Task Delete(Guid id, CancellationToken cancellationToken);
}
