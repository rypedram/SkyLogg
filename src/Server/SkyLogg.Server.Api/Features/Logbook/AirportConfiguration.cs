namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.HasIndex(a => a.ICAO).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(a => a.IATA);
        builder.HasIndex(a => a.CityId);
        builder.HasIndex(a => a.CountryId);

        builder.HasOne(a => a.CityInfo)
            .WithMany(c => c.Airports)
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CountryInfo)
            .WithMany(c => c.Airports)
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Airports_Latitude_Range", "[Latitude] >= -90 AND [Latitude] <= 90");
            t.HasCheckConstraint("CK_Airports_Longitude_Range", "[Longitude] >= -180 AND [Longitude] <= 180");
        });
    }
}
