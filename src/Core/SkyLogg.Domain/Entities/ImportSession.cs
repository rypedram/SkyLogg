using SkyLogg.Domain.Enums;

namespace SkyLogg.Domain.Entities;

public class ImportSession : Common.Entity
{
    public Guid UserId { get; set; }

    public ImportSourceType SourceType { get; set; }

    public string FileName { get; set; } = string.Empty;

    public ImportStatus Status { get; set; } = ImportStatus.Pending;

    public int TotalFlights { get; set; }

    public int SuccessfulFlights { get; set; }

    public int FailedFlights { get; set; }

    public double AverageConfidence { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }
}
