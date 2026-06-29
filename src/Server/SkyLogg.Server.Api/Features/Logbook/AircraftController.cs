using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageAircraft)]
public partial class AircraftController : AppControllerBase, IAircraftController
{
    [HttpGet, EnableQuery]
    public IQueryable<AircraftDto> Get()
    {
        return DbContext.Aircrafts
            .AsNoTracking()
            .NotArchived()
            .Include(a => a.AircraftType)
            .Select(a => new AircraftDto
            {
                Id = a.Id,
                Registration = a.Registration,
                AircraftTypeId = a.AircraftTypeId,
                AircraftTypeDisplay = a.AircraftType == null ? null : a.AircraftType.Manufacturer + " " + a.AircraftType.Model + " " + a.AircraftType.TypeCode,
                Type = a.Type,
                Model = a.Model,
                IsArchived = a.IsArchived,
                Version = a.Version,
            });
    }

    [HttpGet, EnableQuery]
    public IQueryable<AircraftTypeDto> GetAircraftTypes()
    {
        return DbContext.AircraftTypes
            .AsNoTracking()
            .NotArchived()
            .OrderBy(t => t.Manufacturer)
            .ThenBy(t => t.Model)
            .Project();
    }

    [HttpGet("{id}")]
    public async Task<AircraftDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await Get().FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AircraftCouldNotBeFound)]);
    }

    [HttpPost]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAircraft)]
    public async Task<AircraftDto> Create(AircraftDto dto, CancellationToken cancellationToken)
    {
        await PrepareAircraftDto(dto, excludeAircraftId: null, cancellationToken);

        var entity = dto.Map();
        entity.Id = Guid.NewGuid();
        entity.IsArchived = dto.IsArchived;

        await DbContext.Aircrafts.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        return await GetById(entity.Id, cancellationToken);
    }

    [HttpPut]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAircraft)]
    public async Task<AircraftDto> Update(AircraftDto dto, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Aircrafts.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AircraftCouldNotBeFound)]);

        await PrepareAircraftDto(dto, dto.Id, cancellationToken);

        entity.Registration = dto.Registration;
        entity.AircraftTypeId = dto.AircraftTypeId;
        entity.Type = dto.Type;
        entity.Model = dto.Model;
        entity.IsArchived = dto.IsArchived;
        entity.Version = dto.Version;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await DbContext.SaveChangesAsync(cancellationToken);
        return await GetById(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}/{version}")]
    [Authorize(Policy = AppFeatures.BaseInfo.ManageAircraft)]
    public async Task Delete(Guid id, long version, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Aircrafts.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AircraftCouldNotBeFound)]);

        entity.IsArchived = true;
        entity.Version = version;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PrepareAircraftDto(AircraftDto dto, Guid? excludeAircraftId, CancellationToken cancellationToken)
    {
        dto.Registration = dto.Registration?.Trim().ToUpperInvariant();
        dto.Type = dto.Type?.Trim();
        dto.Model = dto.Model?.Trim();

        if (string.IsNullOrWhiteSpace(dto.Registration))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError), Localizer[nameof(AppStrings.Registration)]]);

        if (await DbContext.Aircrafts.NotArchived().AnyAsync(a =>
                a.Registration == dto.Registration &&
                (excludeAircraftId == null || a.Id != excludeAircraftId), cancellationToken))
        {
            throw new ConflictException(Localizer[nameof(AppStrings.AircraftRegistrationAlreadyExists)]);
        }

        if (dto.AircraftTypeId.HasValue)
        {
            var aircraftType = await DbContext.AircraftTypes
                .AsNoTracking()
                .NotArchived()
                .FirstOrDefaultAsync(t => t.Id == dto.AircraftTypeId, cancellationToken)
                ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.AircraftTypeCouldNotBeFound)]);

            dto.Type = aircraftType.Category ?? aircraftType.TypeCode;
            dto.Model = aircraftType.TypeCode;
            dto.AircraftTypeDisplay = $"{aircraftType.Manufacturer} {aircraftType.Model} {aircraftType.TypeCode}";
        }

        if (string.IsNullOrWhiteSpace(dto.Type) || string.IsNullOrWhiteSpace(dto.Model))
            throw new BadRequestException(Localizer[nameof(AppStrings.AircraftTypeCouldNotBeFound)]);
    }
}
