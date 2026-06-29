namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.HasIndex(a => a.Registration).IsUnique();
        builder.HasIndex(a => a.AircraftTypeId);

        builder.HasOne(a => a.AircraftType)
            .WithMany(t => t.Aircraft)
            .HasForeignKey(a => a.AircraftTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        var version = 1L;
        builder.HasData(
            new Aircraft { Id = Guid.Parse("a1000001-0000-4000-8000-000000000001"), Registration = "N172SP", AircraftTypeId = Guid.Parse("b1000001-0000-4000-8000-000000000001"), Type = "ASEL", Model = "C172", IsActive = true, Version = version, CreatedOn = DateTimeOffset.Parse("2026-01-01T00:00:00Z") },
            new Aircraft { Id = Guid.Parse("a1000001-0000-4000-8000-000000000002"), Registration = "N28RT", AircraftTypeId = Guid.Parse("b1000001-0000-4000-8000-000000000002"), Type = "ASEL", Model = "PA-28", IsActive = true, Version = version, CreatedOn = DateTimeOffset.Parse("2026-01-01T00:00:00Z") },
            new Aircraft { Id = Guid.Parse("a1000001-0000-4000-8000-000000000003"), Registration = "N350KA", AircraftTypeId = Guid.Parse("b1000001-0000-4000-8000-000000000003"), Type = "AMEL", Model = "BE-350", IsActive = true, Version = version, CreatedOn = DateTimeOffset.Parse("2026-01-01T00:00:00Z") });
    }
}
