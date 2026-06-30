using FluentValidation;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLogService
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IFlightTimeCalculator flightTimeCalculator = default!;
    [AutoInject] private IStringLocalizer<AppStrings> localizer = default!;
    [AutoInject] private IValidator<FlightLogDto> flightLogValidator = default!;

    public async Task ValidateAndPrepareAsync(FlightLogDto dto, Guid userId, Guid? excludeFlightLogId, CancellationToken cancellationToken)
    {
        var validationResult = await flightLogValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ResourceValidationException(validationResult.Errors
                .Select(e => new LocalizedString(e.ErrorMessage, e.ErrorMessage))
                .ToArray());
        }

        var aircraft = await dbContext.Aircrafts
            .FirstOrDefaultAsync(a => a.Id == dto.AircraftId, cancellationToken)
            ?? throw new ResourceNotFoundException(localizer[nameof(AppStrings.AircraftCouldNotBeFound)]);

        if (aircraft.IsArchived)
            throw new BadRequestException(localizer[nameof(AppStrings.AircraftMustBeActive)]);

        var crewIds = dto.Crew.Select(c => c.CrewMemberId).Distinct().ToArray();
        var ownedCrewCount = await dbContext.CrewMembers
            .CountAsync(c => c.UserId == userId && !c.IsArchived && crewIds.Contains(c.Id), cancellationToken);

        if (ownedCrewCount != crewIds.Length)
            throw new BadRequestException(localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);

        if (crewIds.Length != dto.Crew.Count)
            throw new BadRequestException(localizer[nameof(AppStrings.DuplicateCrewMemberOnFlight)]);

        for (var i = 0; i < dto.Sectors.Count; i++)
        {
            var sector = dto.Sectors[i];
            sector.SectorOrder = i + 1;

            if (sector.DayLandings == 0 && sector.NightLandings == 0)
            {
                sector.DayLandings = 1;
                if (sector.IsNight)
                    sector.NightLandings = 1;
            }

            var (blockMinutes, flightMinutes) = flightTimeCalculator.Calculate(
                sector.BlockOff, sector.BlockOn, sector.Takeoff, sector.Landing);

            sector.BlockTimeMinutes = blockMinutes;
            sector.FlightTimeMinutes = flightMinutes;

            if (sector.PicTimeMinutes + sector.SicTimeMinutes + sector.DualTimeMinutes <= 0)
                sector.PicTimeMinutes = flightMinutes;

            sector.IsNight = sector.IsNight || sector.NightTimeMinutes > 0;
            sector.IsIfr = sector.IsIfr || sector.IfrTimeMinutes > 0;

            if (sector.IsNight && sector.NightTimeMinutes <= 0)
                sector.NightTimeMinutes = flightMinutes;

            if (sector.IsIfr && sector.IfrTimeMinutes <= 0)
                sector.IfrTimeMinutes = flightMinutes;

            if (sector.PicTimeMinutes + sector.SicTimeMinutes + sector.DualTimeMinutes > flightMinutes)
                throw new BadRequestException(localizer[nameof(AppStrings.FlightRoleTimesMustFitFlightTime)]);

            if (sector.NightTimeMinutes > flightMinutes)
                throw new BadRequestException(localizer[nameof(AppStrings.NightTimeMustFitFlightTime)]);

            if (sector.IfrTimeMinutes > flightMinutes)
                throw new BadRequestException(localizer[nameof(AppStrings.IfrTimeMustFitFlightTime)]);
        }

        await EnsureNoAircraftOverlapAsync(dto, excludeFlightLogId, cancellationToken);

        dto.TotalBlockMinutes = dto.Sectors.Sum(s => s.BlockTimeMinutes);
        dto.TotalFlightMinutes = dto.Sectors.Sum(s => s.FlightTimeMinutes);
        dto.TotalPicMinutes = dto.Sectors.Sum(s => s.PicTimeMinutes);
        dto.TotalSicMinutes = dto.Sectors.Sum(s => s.SicTimeMinutes);
        dto.TotalDualMinutes = dto.Sectors.Sum(s => s.DualTimeMinutes);
        dto.TotalNightMinutes = dto.Sectors.Sum(s => s.NightTimeMinutes);
        dto.TotalIfrMinutes = dto.Sectors.Sum(s => s.IfrTimeMinutes);
        dto.TotalLandings = dto.Sectors.Sum(s => s.DayLandings + s.NightLandings);
    }

    private async Task EnsureNoAircraftOverlapAsync(FlightLogDto dto, Guid? excludeFlightLogId, CancellationToken cancellationToken)
    {
        var sectors = dto.Sectors;

        foreach (var sector in sectors)
        {
            var overlapping = await dbContext.FlightSectors
                .Include(s => s.FlightLog)
                .Where(s =>
                    s.FlightLog!.AircraftId == dto.AircraftId &&
                    !s.FlightLog.Deleted &&
                    (excludeFlightLogId == null || s.FlightLogId != excludeFlightLogId) &&
                    sector.BlockOff < s.BlockOn &&
                    sector.BlockOn > s.BlockOff)
                .AnyAsync(cancellationToken);

            if (overlapping)
                throw new ConflictException(localizer[nameof(AppStrings.FlightLogOverlapConflict)]);
        }
    }

    public void ApplyToEntity(FlightLog entity, FlightLogDto dto, Guid userId)
    {
        entity.UserId = userId;
        entity.AircraftId = dto.AircraftId;
        entity.FlightDate = dto.FlightDate;
        entity.Remarks = dto.Remarks;
        entity.TotalBlockMinutes = dto.TotalBlockMinutes;
        entity.TotalFlightMinutes = dto.TotalFlightMinutes;
        entity.TotalPicMinutes = dto.TotalPicMinutes;
        entity.TotalSicMinutes = dto.TotalSicMinutes;
        entity.TotalDualMinutes = dto.TotalDualMinutes;
        entity.TotalNightMinutes = dto.TotalNightMinutes;
        entity.TotalIfrMinutes = dto.TotalIfrMinutes;
        entity.TotalLandings = dto.TotalLandings;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        SyncSectors(entity, dto);
        SyncCrewAssignments(entity, dto);
    }

    private static void SyncSectors(FlightLog entity, FlightLogDto dto)
    {
        var sectorDtos = dto.Sectors.OrderBy(s => s.SectorOrder).ToList();
        var dtoSectorIds = sectorDtos
            .Where(s => s.Id.HasValue)
            .Select(s => s.Id!.Value)
            .ToHashSet();

        foreach (var sector in entity.Sectors.Where(s => !dtoSectorIds.Contains(s.Id)).ToList())
            entity.Sectors.Remove(sector);

        foreach (var sectorDto in sectorDtos)
        {
            var sector = sectorDto.Id.HasValue
                ? entity.Sectors.FirstOrDefault(s => s.Id == sectorDto.Id.Value)
                : null;

            if (sector is null)
            {
                sector = new FlightSector { Id = sectorDto.Id ?? Guid.NewGuid() };
                entity.Sectors.Add(sector);
            }

            ApplySector(sector, sectorDto);
        }
    }

    private static void ApplySector(FlightSector sector, FlightSectorDto sectorDto)
    {
        sector.SectorOrder = sectorDto.SectorOrder;
        sector.DepartureAirportId = sectorDto.DepartureAirportId;
        sector.ArrivalAirportId = sectorDto.ArrivalAirportId;
        sector.BlockOff = sectorDto.BlockOff.ToUniversalTime();
        sector.BlockOn = sectorDto.BlockOn.ToUniversalTime();
        sector.Takeoff = sectorDto.Takeoff?.ToUniversalTime();
        sector.Landing = sectorDto.Landing?.ToUniversalTime();
        sector.BlockTimeMinutes = sectorDto.BlockTimeMinutes;
        sector.FlightTimeMinutes = sectorDto.FlightTimeMinutes;
        sector.PicTimeMinutes = sectorDto.PicTimeMinutes;
        sector.SicTimeMinutes = sectorDto.SicTimeMinutes;
        sector.DualTimeMinutes = sectorDto.DualTimeMinutes;
        sector.NightTimeMinutes = sectorDto.NightTimeMinutes;
        sector.IfrTimeMinutes = sectorDto.IfrTimeMinutes;
        sector.IsIfr = sectorDto.IsIfr;
        sector.IsNight = sectorDto.IsNight;
        sector.DayTakeoffs = sectorDto.DayTakeoffs;
        sector.NightTakeoffs = sectorDto.NightTakeoffs;
        sector.DayLandings = sectorDto.DayLandings;
        sector.NightLandings = sectorDto.NightLandings;
    }

    private static void SyncCrewAssignments(FlightLog entity, FlightLogDto dto)
    {
        var crewMemberIds = dto.Crew.Select(c => c.CrewMemberId).ToHashSet();

        foreach (var assignment in entity.CrewAssignments.Where(c => !crewMemberIds.Contains(c.CrewMemberId)).ToList())
            entity.CrewAssignments.Remove(assignment);

        foreach (var crewDto in dto.Crew)
        {
            var assignment = entity.CrewAssignments.FirstOrDefault(c => c.CrewMemberId == crewDto.CrewMemberId);
            if (assignment is null)
            {
                entity.CrewAssignments.Add(new FlightLogCrew
                {
                    CrewMemberId = crewDto.CrewMemberId,
                    RoleType = crewDto.RoleType,
                });
            }
            else
            {
                assignment.RoleType = crewDto.RoleType;
            }
        }
    }
}
