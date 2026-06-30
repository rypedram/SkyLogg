using SkyLogg.Domain.Enums;

namespace SkyLogg.Domain.Entities;

public class FlightCrew : Common.Entity
{
    public Guid FlightLogId { get; set; }

    public Guid CrewMemberId { get; set; }

    public CrewRoleType RoleType { get; set; }
}
