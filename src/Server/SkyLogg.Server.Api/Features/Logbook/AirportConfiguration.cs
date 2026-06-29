namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.HasIndex(a => a.ICAO).IsUnique();
        builder.HasIndex(a => a.IATA);
        builder.HasIndex(a => a.CountryId);

        builder.HasOne(a => a.CountryInfo)
            .WithMany(c => c.Airports)
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Airports_Latitude_Range", "[Latitude] >= -90 AND [Latitude] <= 90");
            t.HasCheckConstraint("CK_Airports_Longitude_Range", "[Longitude] >= -180 AND [Longitude] <= 180");
        });

        builder.HasData(
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000001"), ICAO = "KJFK", IATA = "JFK", Name = "John F Kennedy Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Country = "United States", City = "New York", Latitude = 40.6413, Longitude = -73.7781, ElevationFt = 13 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000002"), ICAO = "KLAX", IATA = "LAX", Name = "Los Angeles Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Country = "United States", City = "Los Angeles", Latitude = 33.9416, Longitude = -118.4085, ElevationFt = 125 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000003"), ICAO = "KORD", IATA = "ORD", Name = "Chicago O'Hare Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Country = "United States", City = "Chicago", Latitude = 41.9742, Longitude = -87.9073, ElevationFt = 672 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000004"), ICAO = "KATL", IATA = "ATL", Name = "Hartsfield-Jackson Atlanta Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Country = "United States", City = "Atlanta", Latitude = 33.6407, Longitude = -84.4277, ElevationFt = 1026 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000005"), ICAO = "KDEN", IATA = "DEN", Name = "Denver Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000001"), Country = "United States", City = "Denver", Latitude = 39.8561, Longitude = -104.6737, ElevationFt = 5431 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000006"), ICAO = "EGLL", IATA = "LHR", Name = "London Heathrow", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000002"), Country = "United Kingdom", City = "London", Latitude = 51.4700, Longitude = -0.4543, ElevationFt = 83 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000007"), ICAO = "LFPG", IATA = "CDG", Name = "Charles de Gaulle", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000003"), Country = "France", City = "Paris", Latitude = 49.0097, Longitude = 2.5479, ElevationFt = 392 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000008"), ICAO = "EDDF", IATA = "FRA", Name = "Frankfurt Main", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000004"), Country = "Germany", City = "Frankfurt", Latitude = 50.0379, Longitude = 8.5622, ElevationFt = 364 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000009"), ICAO = "OIII", IATA = "THR", Name = "Mehrabad Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000005"), Country = "Iran", City = "Tehran", Latitude = 35.6892, Longitude = 51.3134, ElevationFt = 3962 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000010"), ICAO = "OIIE", IATA = "IKA", Name = "Imam Khomeini Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000005"), Country = "Iran", City = "Tehran", Latitude = 35.4161, Longitude = 51.1522, ElevationFt = 3305 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000011"), ICAO = "OMDB", IATA = "DXB", Name = "Dubai Intl", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000006"), Country = "United Arab Emirates", City = "Dubai", Latitude = 25.2532, Longitude = 55.3657, ElevationFt = 62 },
            new Airport { Id = Guid.Parse("b1000001-0000-4000-8000-000000000012"), ICAO = "LEMD", IATA = "MAD", Name = "Adolfo Suarez Madrid-Barajas", CountryId = Guid.Parse("c1000001-0000-4000-8000-000000000007"), Country = "Spain", City = "Madrid", Latitude = 40.4983, Longitude = -3.5676, ElevationFt = 1998 });
    }
}
