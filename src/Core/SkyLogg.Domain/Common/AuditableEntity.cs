namespace SkyLogg.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public long Version { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
