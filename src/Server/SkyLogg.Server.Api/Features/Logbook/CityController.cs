using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageCities)]
public partial class CityController : AppControllerBase, ICityController
{
    [HttpGet]
    public async Task<List<GeoTimeZoneDto>> GetTimeZones(CancellationToken cancellationToken)
    {
        return await DbContext.GeoTimeZones
            .AsNoTracking()
            .NotArchived()
            .OrderBy(t => t.DisplayName)
            .Project()
            .ToListAsync(cancellationToken);
    }

    [HttpGet]
    public async Task<List<CountryDto>> GetCountries(CancellationToken cancellationToken)
    {
        return await DbContext.Countries
            .AsNoTracking()
            .NotArchived()
            .OrderBy(c => c.Name)
            .Project()
            .ToListAsync(cancellationToken);
    }

    [HttpGet]
    public async Task<PagedResponse<CityDto>> GetCities(CancellationToken cancellationToken)
    {
        return await SearchCitiesInternal(null, cancellationToken);
    }

    [HttpGet("{searchQuery}")]
    public async Task<PagedResponse<CityDto>> SearchCities(string searchQuery, CancellationToken cancellationToken)
    {
        return await SearchCitiesInternal(searchQuery, cancellationToken);
    }

    [HttpGet("{countryId}")]
    public async Task<List<CityDto>> GetCitiesByCountry(Guid countryId, CancellationToken cancellationToken)
    {
        return await DbContext.Cities
            .AsNoTracking()
            .NotArchived()
            .Where(c => c.CountryId == countryId)
            .OrderBy(c => c.Name)
            .Project()
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<CityDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Cities.AsNoTracking().NotArchived().Where(c => c.Id == id).Project()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CityCouldNotBeFound)]);
    }

    [HttpPost]
    public async Task<CityDto> Create(CityDto dto, CancellationToken cancellationToken)
    {
        await PrepareCityDto(dto, excludeCityId: null, cancellationToken);

        var entity = new City
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CountryId = dto.CountryId,
            TimeZoneId = dto.TimeZoneId,
            IsArchived = dto.IsArchived,
        };

        await DbContext.Cities.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpPut]
    public async Task<CityDto> Update(CityDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Cities.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CityCouldNotBeFound)]);

        await PrepareCityDto(dto, dto.Id, cancellationToken);

        entity.Name = dto.Name;
        entity.CountryId = dto.CountryId;
        entity.TimeZoneId = dto.TimeZoneId;
        entity.IsArchived = dto.IsArchived;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Cities.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CityCouldNotBeFound)]);

        entity.IsArchived = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<CityDto>> SearchCitiesInternal(string? query, CancellationToken cancellationToken)
    {
        var cities = DbContext.Cities.AsNoTracking().NotArchived();

        if (string.IsNullOrWhiteSpace(query) is false)
        {
            var term = query.Trim();
            cities = cities.Where(c =>
                (c.Name != null && c.Name.Contains(term)) ||
                (c.CountryInfo != null && c.CountryInfo.Name != null && c.CountryInfo.Name.Contains(term)));
        }

        var items = await cities.OrderBy(c => c.Name).Take(100).Project().ToArrayAsync(cancellationToken);
        return new PagedResponse<CityDto>(items, items.Length);
    }

    private async Task PrepareCityDto(CityDto dto, Guid? excludeCityId, CancellationToken cancellationToken)
    {
        dto.Name = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.City)]]);

        var country = await DbContext.Countries
            .AsNoTracking()
            .NotArchived()
            .FirstOrDefaultAsync(c => c.Id == dto.CountryId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CountryCouldNotBeFound)]);

        var timeZone = await DbContext.GeoTimeZones
            .AsNoTracking()
            .NotArchived()
            .FirstOrDefaultAsync(t => t.Id == dto.TimeZoneId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.TimeZoneCouldNotBeFound)]);

        dto.CountryName = country.Name;
        dto.TimeZoneDisplay = timeZone.DisplayName;

        if (await DbContext.Cities.NotArchived().AnyAsync(c =>
                c.Name == dto.Name &&
                c.CountryId == dto.CountryId &&
                (excludeCityId == null || c.Id != excludeCityId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.CityAlreadyExists)]);
        }
    }
}
