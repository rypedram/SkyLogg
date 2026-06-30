using SkyLogg.Application.Features.Airports.Dtos;
using SkyLogg.Application.Features.Import.Dtos;

namespace SkyLogg.Application.Features.Import.Services;

/// <summary>
/// Orchestrates OCR → AI → validation → review. Never writes to persistence directly.
/// </summary>
public interface IImportOrchestrator
{
    Task<ImportPreviewDto> ProcessAsync(
        Guid sessionId,
        Guid userId,
        Stream source,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<ImportConfirmResultDto> ConfirmAsync(
        Guid sessionId,
        Guid userId,
        IReadOnlyList<ImportFlightCandidateDto> approvedFlights,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Raw text extraction only. No aviation logic.
/// </summary>
public interface IOcrProvider
{
    Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Converts OCR text to structured flight candidates with per-field confidence.
/// Must not access the database.
/// </summary>
public interface IAiExtractionProvider
{
    Task<IReadOnlyList<ImportFlightCandidateDto>> ExtractFlightsAsync(string rawText, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves missing airports from external providers.
/// </summary>
public interface IAirportResolutionProvider
{
    Task<AirportWriteDto?> ResolveAsync(string icaoOrIata, CancellationToken cancellationToken = default);
}
