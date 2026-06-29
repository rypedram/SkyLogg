namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLogCrewConfiguration : IEntityTypeConfiguration<FlightLogCrew>
{
    public void Configure(EntityTypeBuilder<FlightLogCrew> builder)
    {
        builder.HasKey(c => new { c.FlightLogId, c.CrewMemberId });

        builder.HasOne(c => c.CrewMember)
            .WithMany(m => m.FlightAssignments)
            .HasForeignKey(c => c.CrewMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
