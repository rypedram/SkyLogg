namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IAircraftController : IAppController
{
    [HttpGet]
    Task<List<AircraftDto>> Get(CancellationToken cancellationToken) => default!;

    [HttpGet]
    Task<List<AircraftTypeDto>> GetAircraftTypes(CancellationToken cancellationToken) => default!;

    [HttpGet("{id}")]
    Task<AircraftDto> GetById(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<AircraftDto> Create(AircraftDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<AircraftDto> Update(AircraftDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}/{version}")]
    Task Delete(Guid id, long version, CancellationToken cancellationToken);
}
