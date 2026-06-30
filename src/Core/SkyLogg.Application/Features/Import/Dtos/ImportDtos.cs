using SkyLogg.Domain.Enums;

namespace SkyLogg.Application.Features.Import.Dtos;

public sealed class FieldConfidenceDto
{
    public required string FieldName { get; init; }

    public string? Value { get; init; }

    public double Confidence { get; init; }
}

public sealed class ImportFlightCandidateDto
{
    public Guid CandidateId { get; init; } = Guid.NewGuid();

    public DateOnly? FlightDate { get; set; }

    public string? Registration { get; set; }

    public string? AircraftType { get; set; }

    public string? DepartureIcao { get; set; }

    public string? ArrivalIcao { get; set; }

    public string? BlockOff { get; set; }

    public string? Takeoff { get; set; }

    public string? Landing { get; set; }

    public string? BlockOn { get; set; }

    public string? Remarks { get; set; }

    public double OverallConfidence { get; set; }

    public List<FieldConfidenceDto> FieldConfidences { get; set; } = [];
}

public sealed class ImportPreviewDto
{
    public Guid SessionId { get; init; }

    public ImportStatus Status { get; init; }

    public List<ImportFlightCandidateDto> Candidates { get; init; } = [];

    public double AverageConfidence { get; init; }
}

public sealed class ImportSessionDto
{
    public Guid Id { get; init; }

    public ImportSourceType SourceType { get; init; }

    public string FileName { get; init; } = string.Empty;

    public ImportStatus Status { get; init; }

    public int TotalFlights { get; init; }

    public int SuccessfulFlights { get; init; }

    public int FailedFlights { get; init; }

    public double AverageConfidence { get; init; }
}

public sealed class ImportConfirmResultDto
{
    public int SavedCount { get; init; }

    public int SkippedCount { get; init; }
}
