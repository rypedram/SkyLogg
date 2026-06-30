using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Application.Features.Import.Dtos;
using SkyLogg.Application.Features.Import.Services;
using SkyLogg.Domain.Entities;
using SkyLogg.Domain.Enums;

namespace SkyLogg.Infrastructure.Features.Import;

public sealed class ImportOrchestrator : IImportOrchestrator
{
    private readonly IOcrProvider ocr;
    private readonly IAiExtractionProvider ai;
    private readonly IImportSessionRepository sessions;
    private readonly IUnitOfWork unitOfWork;

    public ImportOrchestrator(
        IOcrProvider ocr,
        IAiExtractionProvider ai,
        IImportSessionRepository sessions,
        IUnitOfWork unitOfWork)
    {
        this.ocr = ocr;
        this.ai = ai;
        this.sessions = sessions;
        this.unitOfWork = unitOfWork;
    }

    public async Task<ImportPreviewDto> ProcessAsync(
        Guid sessionId,
        Guid userId,
        Stream source,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetByIdAsync(sessionId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Import session not found.");

        session.Status = ImportStatus.Processing;
        sessions.Update(session);

        try
        {
            var rawText = await ocr.ExtractTextAsync(source, session.FileName, contentType, cancellationToken);
            var candidates = await ai.ExtractFlightsAsync(rawText, cancellationToken);

            session.Status = ImportStatus.AwaitingReview;
            session.TotalFlights = candidates.Count;
            session.AverageConfidence = candidates.Count == 0
                ? 0
                : candidates.Average(c => c.OverallConfidence);

            sessions.Update(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new ImportPreviewDto
            {
                SessionId = sessionId,
                Status = session.Status,
                Candidates = candidates.ToList(),
                AverageConfidence = session.AverageConfidence
            };
        }
        catch
        {
            session.Status = ImportStatus.Failed;
            sessions.Update(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ImportConfirmResultDto> ConfirmAsync(
        Guid sessionId,
        Guid userId,
        IReadOnlyList<ImportFlightCandidateDto> approvedFlights,
        CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetByIdAsync(sessionId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Import session not found.");

        // Persistence of approved flights is delegated to application command handlers (not AI layer).
        session.Status = ImportStatus.Completed;
        session.SuccessfulFlights = approvedFlights.Count;
        session.FailedFlights = session.TotalFlights - approvedFlights.Count;
        session.CompletedAt = DateTimeOffset.UtcNow;

        sessions.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportConfirmResultDto
        {
            SavedCount = approvedFlights.Count,
            SkippedCount = session.FailedFlights
        };
    }
}
