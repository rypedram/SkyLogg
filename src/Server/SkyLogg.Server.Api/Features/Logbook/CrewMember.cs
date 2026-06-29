using SkyLogg.Server.Api.Features.Identity.Models;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CrewMember : IArchivable
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required, MaxLength(100)]
    public string? FirstName { get; set; }

    [Required, MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(50)]
    public string? LicenceNumber { get; set; }

    public DateOnly? Birthday { get; set; }

    public CrewPositionType? PositionType { get; set; }

    public CrewRoleType? DefaultRole { get; set; }

    public bool IsArchived { get; set; }

    public long Version { get; set; }

    public List<FlightLogCrew> FlightAssignments { get; set; } = [];

    [NotMapped]
    public string Name => $"{FirstName} {LastName}".Trim();
}
