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

public partial class FlightImportResolvedAirportDto
{
    public Guid Id { get; set; }

    public string? Icao { get; set; }

    public string? Iata { get; set; }

    public string? Name { get; set; }

    public string? Country { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool WasCreated { get; set; }
}

public partial class FlightImportResolvedAircraftTypeDto
{
    public Guid Id { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? TypeCode { get; set; }

    public string? Category { get; set; }

    public bool WasCreated { get; set; }
}

public partial class FlightImportPreviewDto
{
    public Guid ImportHistoryId { get; set; }

    public FlightImportStatus Status { get; set; }

    public string? RawText { get; set; }

    public List<FlightImportCandidateDto> Candidates { get; set; } = [];

    public List<FlightImportResolvedAirportDto> ResolvedAirports { get; set; } = [];

    public List<FlightImportResolvedAircraftTypeDto> ResolvedAircraftTypes { get; set; } = [];

    public int ValidCount { get; set; }

    public int InvalidCount { get; set; }

    public int DuplicateCount { get; set; }
}

public partial class FlightImportCandidateDto
{
    public int LineNumber { get; set; }

    public DateOnly? FlightDate { get; set; }

    public string? AircraftRegistration { get; set; }

    public string? AircraftType { get; set; }

    public Guid? AircraftId { get; set; }

    public string? DepartureAirportCode { get; set; }

    public Guid? DepartureAirportId { get; set; }

    public string? ArrivalAirportCode { get; set; }

    public Guid? ArrivalAirportId { get; set; }

    public string? BlockOff { get; set; }

    public string? Takeoff { get; set; }

    public string? Landing { get; set; }

    public string? BlockOn { get; set; }

    public string? Remarks { get; set; }

    public int? BlockTimeMinutes { get; set; }

    public int? FlightTimeMinutes { get; set; }

    public double Confidence { get; set; }

    public bool IsValid { get; set; }

    public bool IsSelected { get; set; } = true;

    public bool IsDuplicate { get; set; }

    public List<string> ValidationWarnings { get; set; } = [];

    public FlightLogDto? ProposedFlight { get; set; }
}

public partial class FlightImportConfirmDto
{
    public Guid ImportHistoryId { get; set; }

    public List<FlightLogDto> Flights { get; set; } = [];

    public List<int> LineNumbers { get; set; } = [];
}

public partial class FlightImportConfirmResultDto
{
    public List<FlightLogDto> SavedFlights { get; set; } = [];

    public List<int> SkippedDuplicateLineNumbers { get; set; } = [];

    public int SavedCount { get; set; }

    public int SkippedDuplicateCount { get; set; }
}
