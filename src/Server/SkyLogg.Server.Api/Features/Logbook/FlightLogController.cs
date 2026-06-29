using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightLogController : AppControllerBase, IFlightLogController
{
    [AutoInject] private FlightLogService flightLogService = default!;
    [AutoInject] private LogbookAnalyticsService analyticsService = default!;
    [AutoInject] private FlightLogExportService exportService = default!;

    [HttpGet, EnableQuery]
    public IQueryable<FlightLogDto> Get()
    {
        var userId = User.GetUserId();

        return DbContext.FlightLogs
            .AsNoTracking()
            .Where(f => f.UserId == userId && !f.Deleted)
            .ProjectList();
    }

    [HttpGet]
    public async Task<PagedResponse<FlightLogDto>> GetFlightLogs(ODataQueryOptions<FlightLogDto> odataQuery, CancellationToken cancellationToken)
    {
        var query = (IQueryable<FlightLogDto>)odataQuery.ApplyTo(Get(), ignoreQueryOptions: AllowedQueryOptions.Top | AllowedQueryOptions.Skip);

        var totalCount = await query.LongCountAsync(cancellationToken);

        query = query.SkipIf(odataQuery.Skip is not null, odataQuery.Skip?.Value)
                     .TakeIf(odataQuery.Top is not null, odataQuery.Top?.Value);

        return new PagedResponse<FlightLogDto>(await query.ToArrayAsync(cancellationToken), totalCount);
    }

    [HttpGet("{id}")]
    public async Task<FlightLogDto> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var entity = await DbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Aircraft)
            .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
            .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
            .Include(f => f.CrewAssignments).ThenInclude(c => c.CrewMember)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.Deleted, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.FlightLogCouldNotBeFound)]);

        return LogbookMapper.MapFull(entity);
    }

    [HttpGet]
    public async Task<CurrencyStatusDto> GetCurrencyStatus(CancellationToken cancellationToken)
    {
        return await analyticsService.GetCurrencyStatusAsync(User.GetUserId(), cancellationToken);
    }

    [HttpGet]
    public async Task<FlightSummaryDto> GetSummary(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        return await analyticsService.GetSummaryAsync(User.GetUserId(), fromDate, toDate, cancellationToken);
    }

    [HttpGet]
    public async Task<byte[]> ExportCsv(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        return await exportService.ExportCsvAsync(User.GetUserId(), fromDate, toDate, cancellationToken);
    }

    [HttpGet]
    public async Task<byte[]> ExportPdf(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        return await exportService.ExportPdfAsync(User.GetUserId(), fromDate, toDate, cancellationToken);
    }

    [HttpPost]
    public async Task<FlightLogDto> Create(FlightLogDto dto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        await flightLogService.ValidateAndPrepareAsync(dto, userId, excludeFlightLogId: null, cancellationToken);

        var entity = new FlightLog { Id = Guid.NewGuid() };
        flightLogService.ApplyToEntity(entity, dto, userId);

        await DbContext.FlightLogs.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpPut]
    public async Task<FlightLogDto> Update(FlightLogDto dto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var entity = await DbContext.FlightLogs
            .Include(f => f.Sectors)
            .Include(f => f.CrewAssignments)
            .FirstOrDefaultAsync(f => f.Id == dto.Id && f.UserId == userId && !f.Deleted, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.FlightLogCouldNotBeFound)]);

        await flightLogService.ValidateAndPrepareAsync(dto, userId, dto.Id, cancellationToken);

        flightLogService.ApplyToEntity(entity, dto, userId);
        entity.Version = dto.Version;

        await DbContext.SaveChangesAsync(cancellationToken);

        return await Get(entity.Id, cancellationToken);
    }

    [HttpDelete("{id}/{version}")]
    public async Task Delete(Guid id, long version, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var entity = await DbContext.FlightLogs
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.Deleted, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.FlightLogCouldNotBeFound)]);

        entity.Deleted = true;
        entity.Version = version;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
