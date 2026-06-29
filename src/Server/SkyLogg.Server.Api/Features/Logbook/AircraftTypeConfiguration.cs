namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AircraftTypeConfiguration : IEntityTypeConfiguration<AircraftType>
{
    public void Configure(EntityTypeBuilder<AircraftType> builder)
    {
        builder.HasIndex(t => new { t.Manufacturer, t.Model, t.TypeCode }).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(t => t.TypeCode);

        var version = 1L;
        var createdOn = DateTimeOffset.Parse("2026-01-01T00:00:00Z");

        builder.HasData(
            new AircraftType { Id = Guid.Parse("b1000001-0000-4000-8000-000000000001"), Manufacturer = "Cessna", Model = "172 Skyhawk", TypeCode = "C172", Category = "ASEL", EngineType = "Piston", IsArchived = false, Version = version, CreatedOn = createdOn },
            new AircraftType { Id = Guid.Parse("b1000001-0000-4000-8000-000000000002"), Manufacturer = "Piper", Model = "PA-28 Cherokee", TypeCode = "PA28", Category = "ASEL", EngineType = "Piston", IsArchived = false, Version = version, CreatedOn = createdOn },
            new AircraftType { Id = Guid.Parse("b1000001-0000-4000-8000-000000000003"), Manufacturer = "Beechcraft", Model = "King Air 350", TypeCode = "BE350", Category = "AMEL", EngineType = "Turboprop", IsArchived = false, Version = version, CreatedOn = createdOn });
    }
}
