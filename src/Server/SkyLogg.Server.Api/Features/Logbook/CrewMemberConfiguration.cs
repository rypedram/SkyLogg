namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
{
    public void Configure(EntityTypeBuilder<CrewMember> builder)
    {
        builder.HasIndex(c => new { c.UserId, c.FirstName, c.LastName });
        builder.HasIndex(c => c.LicenceNumber);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
