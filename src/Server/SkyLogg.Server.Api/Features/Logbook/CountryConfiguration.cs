namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasIndex(c => c.Iso2).IsUnique();
        builder.HasIndex(c => c.Iso3).IsUnique();
        builder.HasIndex(c => c.Name);

        builder.HasData(
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Iso2 = "US", Iso3 = "USA", Name = "United States" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000002"), Iso2 = "GB", Iso3 = "GBR", Name = "United Kingdom" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000003"), Iso2 = "FR", Iso3 = "FRA", Name = "France" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000004"), Iso2 = "DE", Iso3 = "DEU", Name = "Germany" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000005"), Iso2 = "IR", Iso3 = "IRN", Name = "Iran" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000006"), Iso2 = "AE", Iso3 = "ARE", Name = "United Arab Emirates" },
            new Country { Id = Guid.Parse("c1000001-0000-4000-8000-000000000007"), Iso2 = "ES", Iso3 = "ESP", Name = "Spain" });
    }
}
