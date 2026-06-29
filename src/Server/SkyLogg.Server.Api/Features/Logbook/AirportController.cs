using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
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
            .OrderBy(c => c.Name)
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
        return await DbContext.Airports.AsNoTracking().Where(a => a.Id == id).Project()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);
    }

    [HttpPost]
    [Authorize(Policy = AppFeatures.Logbook.ManageFleet)]
    public async Task<AirportDto> Create(AirportDto dto, CancellationToken cancellationToken)
    {
        await PrepareAirportDto(dto, excludeAirportId: null, cancellationToken);

        var entity = new Airport
        {
            Id = Guid.NewGuid(),
            IATA = dto.IATA,
            ICAO = dto.ICAO,
            Name = dto.Name,
            CountryId = dto.CountryId,
            Country = dto.Country,
            City = dto.City,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ElevationFt = dto.ElevationFt,
            IsActive = dto.IsActive,
        };

        await DbContext.Airports.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpPut]
    [Authorize(Policy = AppFeatures.Logbook.ManageFleet)]
    public async Task<AirportDto> Update(AirportDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Airports.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);

        await PrepareAirportDto(dto, dto.Id, cancellationToken);

        entity.IATA = dto.IATA;
        entity.ICAO = dto.ICAO;
        entity.Name = dto.Name;
        entity.CountryId = dto.CountryId;
        entity.Country = dto.Country;
        entity.City = dto.City;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.ElevationFt = dto.ElevationFt;
        entity.IsActive = dto.IsActive;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppFeatures.Logbook.ManageFleet)]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Airports.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AirportCouldNotBeFound)]);

        entity.IsActive = false;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<AirportDto>> SearchAirportsInternal(string? query, CancellationToken cancellationToken)
    {
        var airports = DbContext.Airports.AsNoTracking().Where(a => a.IsActive);

        if (string.IsNullOrWhiteSpace(query) is false)
        {
            var term = query.Trim();
            airports = airports.Where(a =>
                (a.ICAO != null && a.ICAO.Contains(term)) ||
                (a.IATA != null && a.IATA.Contains(term)) ||
                (a.Name != null && a.Name.Contains(term)) ||
                (a.City != null && a.City.Contains(term)) ||
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
        dto.City = dto.City?.Trim();
        dto.Country = dto.Country?.Trim();

        if (string.IsNullOrWhiteSpace(dto.ICAO) || string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError)]);

        if (dto.Latitude is < -90 or > 90 || dto.Longitude is < -180 or > 180)
            throw new BadRequestException(Localizer[nameof(AppStrings.InvalidAirportCoordinates)]);

        if (await DbContext.Airports.AnyAsync(a =>
                a.ICAO == dto.ICAO &&
                (excludeAirportId == null || a.Id != excludeAirportId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.AirportIcaoAlreadyExists)]);
        }

        if (dto.CountryId.HasValue)
        {
            var country = await DbContext.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.CountryId, cancellationToken)
                ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CountryCouldNotBeFound)]);

            dto.Country = country.Name;
        }
    }
}
