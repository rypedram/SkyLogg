using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageCountries)]
public partial class CountryController : AppControllerBase, ICountryController
{
    [HttpGet]
    public async Task<PagedResponse<CountryDto>> GetCountries(CancellationToken cancellationToken)
    {
        return await SearchCountriesInternal(null, cancellationToken);
    }

    [HttpGet("{searchQuery}")]
    public async Task<PagedResponse<CountryDto>> SearchCountries(string searchQuery, CancellationToken cancellationToken)
    {
        return await SearchCountriesInternal(searchQuery, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<CountryDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Countries.AsNoTracking().NotArchived().Where(c => c.Id == id).Project()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CountryCouldNotBeFound)]);
    }

    [HttpPost]
    public async Task<CountryDto> Create(CountryDto dto, CancellationToken cancellationToken)
    {
        await PrepareCountryDto(dto, excludeCountryId: null, cancellationToken);

        var entity = new Country
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Iso2 = dto.Iso2,
            Iso3 = dto.Iso3,
            IsArchived = false,
        };

        await DbContext.Countries.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpPut]
    public async Task<CountryDto> Update(CountryDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Countries.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CountryCouldNotBeFound)]);

        await PrepareCountryDto(dto, dto.Id, cancellationToken);

        entity.Name = dto.Name;
        entity.Iso2 = dto.Iso2;
        entity.Iso3 = dto.Iso3;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Countries.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CountryCouldNotBeFound)]);

        entity.IsArchived = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<CountryDto>> SearchCountriesInternal(string? query, CancellationToken cancellationToken)
    {
        var countries = DbContext.Countries.AsNoTracking().NotArchived();

        if (string.IsNullOrWhiteSpace(query) is false)
        {
            var term = query.Trim();
            countries = countries.Where(c =>
                (c.Name != null && c.Name.Contains(term)) ||
                (c.Iso2 != null && c.Iso2.Contains(term)) ||
                (c.Iso3 != null && c.Iso3.Contains(term)));
        }

        var items = await countries.OrderBy(c => c.Name).Take(100).Project().ToArrayAsync(cancellationToken);
        return new PagedResponse<CountryDto>(items, items.Length);
    }

    private async Task PrepareCountryDto(CountryDto dto, Guid? excludeCountryId, CancellationToken cancellationToken)
    {
        dto.Name = dto.Name?.Trim();
        dto.Iso2 = dto.Iso2?.Trim().ToUpperInvariant();
        dto.Iso3 = dto.Iso3?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.Country)]]);

        if (string.IsNullOrWhiteSpace(dto.Iso2))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.Iso2)]]);

        if (string.IsNullOrWhiteSpace(dto.Iso3))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.Iso3)]]);

        if (await DbContext.Countries.NotArchived().AnyAsync(c =>
                c.Iso2 == dto.Iso2 &&
                (excludeCountryId == null || c.Id != excludeCountryId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.CountryIso2AlreadyExists)]);
        }

        if (await DbContext.Countries.NotArchived().AnyAsync(c =>
                c.Iso3 == dto.Iso3 &&
                (excludeCountryId == null || c.Id != excludeCountryId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.CountryIso3AlreadyExists)]);
        }
    }
}
