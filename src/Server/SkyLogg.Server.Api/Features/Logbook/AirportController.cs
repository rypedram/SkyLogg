using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageAirports)]
public partial class AirportController : AppControllerBase, IAirportController
{
    [HttpGet]
    public async Task<PagedResponse<AirportDto>> GetAirports(CancellationToken cancellationToken)
    {
        return await SearchAirportsInternal(null, cancellationToken);
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
    public async Task<List<CityDto>> GetCities(CancellationToken cancellationToken)
    {
        return await DbContext.Cities
            .AsNoTracking()
            .NotArchived()
            .OrderBy(c => c.Name)
            .Take(500)
            .Project()
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{searchQuery}")]
    public async Task<PagedResponse<AirportDto>> SearchAirports(string searchQuery, CancellationToken cancellationToken)
    {
        return await SearchAirportsInternal(searchQuery, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<AirportDto> Get(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Airports.AsNoTracking().NotArchived().Where(a => a.Id == id).Project()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);
    }

    [HttpPost]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAirports)]
    public async Task<AirportDto> Create(AirportDto dto, CancellationToken cancellationToken)
    {
        await PrepareAirportDto(dto, excludeAirportId: null, cancellationToken);

        var entity = new Airport
        {
            Id = Guid.NewGuid(),
            IATA = dto.IATA,
            ICAO = dto.ICAO,
            Name = dto.Name,
            CityId = dto.CityId,
            CountryId = dto.CountryId,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ElevationFt = dto.ElevationFt,
            IsArchived = dto.IsArchived,
        };

        await DbContext.Airports.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpPut]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAirports)]
    public async Task<AirportDto> Update(AirportDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Airports.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);

        await PrepareAirportDto(dto, dto.Id, cancellationToken);

        entity.IATA = dto.IATA;
        entity.ICAO = dto.ICAO;
        entity.Name = dto.Name;
        entity.CityId = dto.CityId;
        entity.CountryId = dto.CountryId;
        entity.Country = dto.Country;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.ElevationFt = dto.ElevationFt;
        entity.IsArchived = dto.IsArchived;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAirports)]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Airports.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);

        entity.IsArchived = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<AirportDto>> SearchAirportsInternal(string? query, CancellationToken cancellationToken)
    {
        var airports = DbContext.Airports.AsNoTracking().NotArchived();

        if (string.IsNullOrWhiteSpace(query) is false)
        {
            var term = query.Trim();
            airports = airports.Where(a =>
                (a.ICAO != null && a.ICAO.Contains(term)) ||
                (a.IATA != null && a.IATA.Contains(term)) ||
                (a.Name != null && a.Name.Contains(term)) ||
                (a.CityInfo != null && a.CityInfo.Name != null && a.CityInfo.Name.Contains(term)) ||
                (a.Country != null && a.Country.Contains(term)));
        }

        var items = await airports.OrderBy(a => a.ICAO).Take(50).Project().ToArrayAsync(cancellationToken);
        return new PagedResponse<AirportDto>(items, items.Length);
    }

    private async Task PrepareAirportDto(AirportDto dto, Guid? excludeAirportId, CancellationToken cancellationToken)
    {
        dto.ICAO = dto.ICAO?.Trim().ToUpperInvariant();
        dto.IATA = string.IsNullOrWhiteSpace(dto.IATA) ? null : dto.IATA.Trim().ToUpperInvariant();
        dto.Name = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(dto.ICAO) || string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError)]);

        if (dto.Latitude is < -90 or > 90 || dto.Longitude is < -180 or > 180)
            throw new BadRequestException(Localizer[nameof(AppStrings.InvalidAirportCoordinates)]);

        if (await DbContext.Airports.NotArchived().AnyAsync(a =>
                a.ICAO == dto.ICAO &&
                (excludeAirportId == null || a.Id != excludeAirportId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.AirportIcaoAlreadyExists)]);
        }

        var city = await DbContext.Cities
            .AsNoTracking()
            .Include(c => c.CountryInfo)
            .Include(c => c.TimeZone)
            .NotArchived()
            .FirstOrDefaultAsync(c => c.Id == dto.CityId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CityCouldNotBeFound)]);

        dto.City = city.Name;
        dto.CountryId = city.CountryId;
        dto.Country = city.CountryInfo?.Name;
        dto.TimeZoneDisplay = city.TimeZone?.DisplayName;
    }
}
