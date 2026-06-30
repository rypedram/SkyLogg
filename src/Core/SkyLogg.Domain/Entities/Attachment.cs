namespace SkyLogg.Domain.Entities;

public class Attachment : Common.Entity
{
    public Guid FlightLogId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
