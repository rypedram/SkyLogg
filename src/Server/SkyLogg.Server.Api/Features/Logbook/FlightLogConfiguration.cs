namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLogConfiguration : IEntityTypeConfiguration<FlightLog>
{
    public void Configure(EntityTypeBuilder<FlightLog> builder)
    {
        builder.HasIndex(f => new { f.UserId, f.FlightDate });
        builder.HasIndex(f => f.AircraftId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FlightLogs_TotalBlockMinutes_NonNegative", "[TotalBlockMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalFlightMinutes_NonNegative", "[TotalFlightMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalPicMinutes_NonNegative", "[TotalPicMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalSicMinutes_NonNegative", "[TotalSicMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalDualMinutes_NonNegative", "[TotalDualMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalNightMinutes_NonNegative", "[TotalNightMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalIfrMinutes_NonNegative", "[TotalIfrMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightLogs_TotalLandings_NonNegative", "[TotalLandings] >= 0");
        });

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Aircraft)
            .WithMany(a => a.FlightLogs)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Sectors)
            .WithOne(s => s.FlightLog)
            .HasForeignKey(s => s.FlightLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.CrewAssignments)
            .WithOne(c => c.FlightLog)
            .HasForeignKey(c => c.FlightLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
