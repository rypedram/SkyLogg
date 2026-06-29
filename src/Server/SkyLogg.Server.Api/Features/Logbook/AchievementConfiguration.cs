namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasIndex(a => a.Code).IsUnique();

        builder.HasData(
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000001"), Code = "HOURS_100", Name = "First 100 hours", Description = "Log 100 total flight hours.", Metric = "TotalFlightHours", Threshold = 100 },
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000002"), Code = "INTERNATIONAL_1", Name = "First international flight", Description = "Log a flight touching at least two countries.", Metric = "InternationalFlights", Threshold = 1 },
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000003"), Code = "AIRPORTS_10", Name = "10 different airports", Description = "Visit 10 distinct airports.", Metric = "DistinctAirports", Threshold = 10 },
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000004"), Code = "AIRCRAFT_5", Name = "5 aircraft types", Description = "Fly 5 different aircraft.", Metric = "DistinctAircraft", Threshold = 5 },
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000005"), Code = "NIGHT_25", Name = "Night flying milestone", Description = "Log 25 night flight hours.", Metric = "NightHours", Threshold = 25 },
            new Achievement { Id = Guid.Parse("d1000001-0000-4000-8000-000000000006"), Code = "IFR_25", Name = "IFR milestone", Description = "Log 25 IFR hours.", Metric = "IfrHours", Threshold = 25 });
    }
}
