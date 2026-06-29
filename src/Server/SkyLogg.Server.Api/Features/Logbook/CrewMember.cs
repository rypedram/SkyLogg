using SkyLogg.Server.Api.Features.Identity.Models;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class CrewMember
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required, MaxLength(200)]
    public string? Name { get; set; }

    public CrewRoleType? DefaultRole { get; set; }

    public long Version { get; set; }

    public List<FlightLogCrew> FlightAssignments { get; set; } = [];
}
