namespace SkyLogg.Shared.Features.Logbook;

public enum FlightImportSourceType
{
    Camera = 0,
    Image = 1,
    Pdf = 2,
    Csv = 3,
    Excel = 4,
    RawText = 5,
}

public enum FlightImportStatus
{
    Uploaded = 0,
    OcrCompleted = 1,
    AiParsed = 2,
    Confirmed = 3,
    Failed = 4,
}

public partial class FlightImportRequestDto
{
    public FlightImportSourceType SourceType { get; set; } = FlightImportSourceType.RawText;

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public string? RawText { get; set; }

    public string? FileName { get; set; }
}

public partial class FlightImportPreviewDto
{
    public Guid ImportHistoryId { get; set; }

    public FlightImportStatus Status { get; set; }

    public string? RawText { get; set; }

    public List<FlightImportCandidateDto> Candidates { get; set; } = [];
}

public partial class FlightImportCandidateDto
{
    public DateOnly? FlightDate { get; set; }

    public string? AircraftRegistration { get; set; }

    public string? DepartureAirportCode { get; set; }

    public string? ArrivalAirportCode { get; set; }

    public string? BlockOff { get; set; }

    public string? Takeoff { get; set; }

    public string? Landing { get; set; }

    public string? BlockOn { get; set; }

    public string? Remarks { get; set; }

    public double Confidence { get; set; }
}

public partial class FlightImportConfirmDto
{
    public Guid ImportHistoryId { get; set; }

    public List<FlightLogDto> Flights { get; set; } = [];
}
