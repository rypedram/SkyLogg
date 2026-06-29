using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLogCrew
{
    public Guid FlightLogId { get; set; }

    [ForeignKey(nameof(FlightLogId))]
    public FlightLog? FlightLog { get; set; }

    public Guid CrewMemberId { get; set; }

    [ForeignKey(nameof(CrewMemberId))]
    public CrewMember? CrewMember { get; set; }

    public CrewRoleType RoleType { get; set; }
}
