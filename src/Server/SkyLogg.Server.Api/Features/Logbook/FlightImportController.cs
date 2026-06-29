using System.Text.Json;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightImportController : AppControllerBase, IFlightImportController
{
    [AutoInject] private IAiFlightExtractionService aiFlightExtractionService = default!;
    [AutoInject] private FlightLogService flightLogService = default!;

    [HttpPost]
    public async Task<FlightImportPreviewDto> Preview(FlightImportRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError)]);

        var userId = User.GetUserId();
        var candidates = await aiFlightExtractionService.ExtractFlightsAsync(request.RawText, cancellationToken);
        var importHistory = new ImportHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceType = request.SourceType,
            FileName = request.FileName,
            Status = FlightImportStatus.AiParsed,
            RawText = request.RawText,
            ParsedJson = JsonSerializer.Serialize(candidates),
        };

        await DbContext.ImportHistories.AddAsync(importHistory, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return new FlightImportPreviewDto
        {
            ImportHistoryId = importHistory.Id,
            Status = importHistory.Status,
            RawText = importHistory.RawText,
            Candidates = candidates,
        };
    }

    [HttpPost]
    public async Task<List<FlightLogDto>> Confirm(FlightImportConfirmDto request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var importHistory = await DbContext.ImportHistories
            .FirstOrDefaultAsync(i => i.Id == request.ImportHistoryId && i.UserId == userId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.ImportHistoryCouldNotBeFound)]);

        var savedFlights = new List<FlightLogDto>();
        foreach (var flight in request.Flights)
        {
            await flightLogService.ValidateAndPrepareAsync(flight, userId, excludeFlightLogId: null, cancellationToken);

            var entity = new FlightLog { Id = Guid.NewGuid() };
            flightLogService.ApplyToEntity(entity, flight, userId);
            await DbContext.FlightLogs.AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);

            savedFlights.Add(LogbookMapper.MapFull(await DbContext.FlightLogs
                .AsNoTracking()
                .Include(f => f.Aircraft)
                .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
                .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
                .Include(f => f.CrewAssignments).ThenInclude(c => c.CrewMember)
                .FirstAsync(f => f.Id == entity.Id, cancellationToken)));
        }

        importHistory.Status = FlightImportStatus.Confirmed;
        importHistory.CompletedOn = DateTimeOffset.UtcNow;
        await DbContext.SaveChangesAsync(cancellationToken);

        return savedFlights;
    }
}
