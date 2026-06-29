namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasIndex(c => new { c.CountryId, c.Name }).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(c => c.TimeZoneId);

        builder.HasOne(c => c.CountryInfo)
            .WithMany(c => c.Cities)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.TimeZone)
            .WithMany(t => t.Cities)
            .HasForeignKey(c => c.TimeZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
