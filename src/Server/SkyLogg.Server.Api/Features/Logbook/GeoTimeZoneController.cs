using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageTimeZones)]
public partial class GeoTimeZoneController : AppControllerBase, IGeoTimeZoneController
{
    [HttpGet]
    public async Task<PagedResponse<GeoTimeZoneDto>> GetTimeZones(CancellationToken cancellationToken)
    {
        return await SearchTimeZonesInternal(null, cancellationToken);
    }

    [HttpGet("{searchQuery}")]
    public async Task<PagedResponse<GeoTimeZoneDto>> SearchTimeZones(string searchQuery, CancellationToken cancellationToken)
    {
        return await SearchTimeZonesInternal(searchQuery, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<GeoTimeZoneDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.GeoTimeZones.AsNoTracking().NotArchived().Where(t => t.Id == id).Project()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.TimeZoneCouldNotBeFound)]);
    }

    [HttpPost]
    public async Task<GeoTimeZoneDto> Create(GeoTimeZoneDto dto, CancellationToken cancellationToken)
    {
        await PrepareTimeZoneDto(dto, excludeTimeZoneId: null, cancellationToken);

        var entity = new GeoTimeZone
        {
            Id = Guid.NewGuid(),
            IanaId = dto.IanaId,
            DisplayName = dto.DisplayName,
            UtcOffset = dto.UtcOffset,
            IsArchived = false,
        };

        await DbContext.GeoTimeZones.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpPut]
    public async Task<GeoTimeZoneDto> Update(GeoTimeZoneDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.GeoTimeZones.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.TimeZoneCouldNotBeFound)]);

        await PrepareTimeZoneDto(dto, dto.Id, cancellationToken);

        entity.IanaId = dto.IanaId;
        entity.DisplayName = dto.DisplayName;
        entity.UtcOffset = dto.UtcOffset;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await GetById(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.GeoTimeZones.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.TimeZoneCouldNotBeFound)]);

        entity.IsArchived = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<GeoTimeZoneDto>> SearchTimeZonesInternal(string? query, CancellationToken cancellationToken)
    {
        var timeZones = DbContext.GeoTimeZones.AsNoTracking().NotArchived();

        if (string.IsNullOrWhiteSpace(query) is false)
        {
            var term = query.Trim();
            timeZones = timeZones.Where(t =>
                (t.IanaId != null && t.IanaId.Contains(term)) ||
                (t.DisplayName != null && t.DisplayName.Contains(term)) ||
                (t.UtcOffset != null && t.UtcOffset.Contains(term)));
        }

        var items = await timeZones.OrderBy(t => t.DisplayName).Take(100).Project().ToArrayAsync(cancellationToken);
        return new PagedResponse<GeoTimeZoneDto>(items, items.Length);
    }

    private async Task PrepareTimeZoneDto(GeoTimeZoneDto dto, Guid? excludeTimeZoneId, CancellationToken cancellationToken)
    {
        dto.IanaId = dto.IanaId?.Trim();
        dto.DisplayName = dto.DisplayName?.Trim();
        dto.UtcOffset = dto.UtcOffset?.Trim();

        if (string.IsNullOrWhiteSpace(dto.IanaId))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.IanaTimeZoneId)]]);

        if (string.IsNullOrWhiteSpace(dto.DisplayName))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.TimeZoneDisplayName)]]);

        if (await DbContext.GeoTimeZones.NotArchived().AnyAsync(t =>
                t.IanaId == dto.IanaId &&
                (excludeTimeZoneId == null || t.Id != excludeTimeZoneId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.TimeZoneIanaAlreadyExists)]);
        }
    }
}
