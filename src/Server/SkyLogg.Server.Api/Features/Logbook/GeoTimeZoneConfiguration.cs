namespace SkyLogg.Server.Api.Features.Logbook;

public partial class GeoTimeZoneConfiguration : IEntityTypeConfiguration<GeoTimeZone>
{
    public void Configure(EntityTypeBuilder<GeoTimeZone> builder)
    {
        builder.HasIndex(t => t.IanaId).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(t => t.DisplayName);
    }
}
