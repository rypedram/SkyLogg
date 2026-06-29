using SkyLogg.Server.Api.Features.Identity.Models;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class ImportHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public FlightImportSourceType SourceType { get; set; }

    [MaxLength(260)]
    public string? FileName { get; set; }

    public FlightImportStatus Status { get; set; }

    public string? RawText { get; set; }

    public string? ParsedJson { get; set; }

    [MaxLength(2000)]
    public string? Error { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedOn { get; set; }
}
