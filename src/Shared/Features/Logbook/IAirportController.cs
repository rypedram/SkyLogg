namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface IAirportController : IAppController
{
    [HttpGet]
    Task<PagedResponse<AirportDto>> GetAirports(CancellationToken cancellationToken);

    [HttpGet]
    Task<List<CountryDto>> GetCountries(CancellationToken cancellationToken);

    [HttpGet("{searchQuery}")]
    Task<PagedResponse<AirportDto>> SearchAirports(string searchQuery, CancellationToken cancellationToken);

    [HttpGet("{id}")]
    Task<AirportDto> Get(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<AirportDto> Create(AirportDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<AirportDto> Update(AirportDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}")]
    Task Delete(Guid id, CancellationToken cancellationToken);
}
