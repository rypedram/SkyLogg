namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightSectorConfiguration : IEntityTypeConfiguration<FlightSector>
{
    public void Configure(EntityTypeBuilder<FlightSector> builder)
    {
        builder.HasIndex(s => new { s.FlightLogId, s.SectorOrder }).IsUnique();
        builder.HasIndex(s => new { s.DepartureAirportId, s.ArrivalAirportId });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FlightSectors_BlockTimeMinutes_NonNegative", "[BlockTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_FlightTimeMinutes_NonNegative", "[FlightTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_PicTimeMinutes_NonNegative", "[PicTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_SicTimeMinutes_NonNegative", "[SicTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_DualTimeMinutes_NonNegative", "[DualTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_NightTimeMinutes_NonNegative", "[NightTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_IfrTimeMinutes_NonNegative", "[IfrTimeMinutes] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_DayLandings_NonNegative", "[DayLandings] >= 0");
            t.HasCheckConstraint("CK_FlightSectors_NightLandings_NonNegative", "[NightLandings] >= 0");
        });

        builder.HasOne(s => s.DepartureAirport)
            .WithMany()
            .HasForeignKey(s => s.DepartureAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ArrivalAirport)
            .WithMany()
            .HasForeignKey(s => s.ArrivalAirportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
