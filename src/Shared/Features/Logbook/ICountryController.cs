namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface ICountryController : IAppController
{
    [HttpGet]
    Task<PagedResponse<CountryDto>> GetCountries(CancellationToken cancellationToken);

    [HttpGet("{searchQuery}")]
    Task<PagedResponse<CountryDto>> SearchCountries(string searchQuery, CancellationToken cancellationToken);

    [HttpGet("{id}")]
    Task<CountryDto> GetById(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<CountryDto> Create(CountryDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<CountryDto> Update(CountryDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}")]
    Task Delete(Guid id, CancellationToken cancellationToken);
}
