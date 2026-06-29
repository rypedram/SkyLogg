namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasIndex(c => c.Iso2).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(c => c.Iso3).IsUnique().HasFilter(ArchivableQueries.NotArchivedIndexFilter);
        builder.HasIndex(c => c.Name);
    }
}
