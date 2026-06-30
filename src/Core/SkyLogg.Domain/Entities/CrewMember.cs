namespace SkyLogg.Domain.Entities;

public class CrewMember : Common.AuditableEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
}
