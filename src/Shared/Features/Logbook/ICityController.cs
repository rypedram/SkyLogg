namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface ICityController : IAppController
{
    [HttpGet]
    Task<List<GeoTimeZoneDto>> GetTimeZones(CancellationToken cancellationToken);

    [HttpGet]
    Task<List<CountryDto>> GetCountries(CancellationToken cancellationToken);

    [HttpGet]
    Task<PagedResponse<CityDto>> GetCities(CancellationToken cancellationToken);

    [HttpGet("{searchQuery}")]
    Task<PagedResponse<CityDto>> SearchCities(string searchQuery, CancellationToken cancellationToken);

    [HttpGet("{countryId}")]
    Task<List<CityDto>> GetCitiesByCountry(Guid countryId, CancellationToken cancellationToken);

    [HttpGet("{id}")]
    Task<CityDto> GetById(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<CityDto> Create(CityDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<CityDto> Update(CityDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}")]
    Task Delete(Guid id, CancellationToken cancellationToken);
}
