namespace SkyLogg.Server.Api.Features.Logbook.Import;

public partial class AircraftTypeResolutionService : IAircraftTypeResolutionService
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IExternalAviationDataProvider externalProvider = default!;

    public async Task<(AircraftType AircraftType, bool WasCreated)> ResolveAircraftTypeAsync(string aircraftDescription, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aircraftDescription))
            throw new BadRequestException("Aircraft description is required.");

        var normalized = aircraftDescription.Trim();
        var typeCode = ExtractTypeCode(normalized);

        var existing = await dbContext.AircraftTypes
            .NotArchived()
            .FirstOrDefaultAsync(t =>
                t.TypeCode == typeCode ||
                (t.Manufacturer + " " + t.Model).ToUpper() == normalized.ToUpper(), cancellationToken);

        if (existing is not null)
            return (existing, false);

        var external = await externalProvider.ResolveAircraftTypeAsync(normalized, cancellationToken)
            ?? new ResolvedAircraftTypeInfo
            {
                Manufacturer = "Unknown",
                Model = normalized,
                TypeCode = typeCode,
                Category = "Unknown",
                EngineType = "Unknown"
            };

        var duplicate = await dbContext.AircraftTypes
            .NotArchived()
            .FirstOrDefaultAsync(t => t.TypeCode == external.TypeCode, cancellationToken);

        if (duplicate is not null)
            return (duplicate, false);

        var aircraftType = new AircraftType
        {
            Id = Guid.NewGuid(),
            Manufacturer = external.Manufacturer,
            Model = external.Model,
            TypeCode = external.TypeCode,
            Category = external.Category,
            EngineType = external.EngineType,
            IsArchived = false
        };

        await dbContext.AircraftTypes.AddAsync(aircraftType, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (aircraftType, true);
    }

    private static string ExtractTypeCode(string description)
    {
        var parts = description.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts[^1].ToUpperInvariant() : description.ToUpperInvariant();
    }
}
