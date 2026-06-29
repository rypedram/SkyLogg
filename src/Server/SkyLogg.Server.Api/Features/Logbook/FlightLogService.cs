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

        if (!aircraft.IsActive)
            throw new BadRequestException(localizer[nameof(AppStrings.AircraftMustBeActive)]);

        var crewIds = dto.Crew.Select(c => c.CrewMemberId).Distinct().ToArray();
        var ownedCrewCount = await dbContext.CrewMembers
            .CountAsync(c => c.UserId == userId && crewIds.Contains(c.Id), cancellationToken);

        if (ownedCrewCount != crewIds.Length)
            throw new BadRequestException(localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);

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

        entity.Sectors.Clear();
        foreach (var sectorDto in dto.Sectors.OrderBy(s => s.SectorOrder))
        {
            entity.Sectors.Add(new FlightSector
            {
                Id = sectorDto.Id ?? Guid.NewGuid(),
                SectorOrder = sectorDto.SectorOrder,
                DepartureAirportId = sectorDto.DepartureAirportId,
                ArrivalAirportId = sectorDto.ArrivalAirportId,
                BlockOff = sectorDto.BlockOff.ToUniversalTime(),
                BlockOn = sectorDto.BlockOn.ToUniversalTime(),
                Takeoff = sectorDto.Takeoff?.ToUniversalTime(),
                Landing = sectorDto.Landing?.ToUniversalTime(),
                BlockTimeMinutes = sectorDto.BlockTimeMinutes,
                FlightTimeMinutes = sectorDto.FlightTimeMinutes,
                PicTimeMinutes = sectorDto.PicTimeMinutes,
                SicTimeMinutes = sectorDto.SicTimeMinutes,
                DualTimeMinutes = sectorDto.DualTimeMinutes,
                NightTimeMinutes = sectorDto.NightTimeMinutes,
                IfrTimeMinutes = sectorDto.IfrTimeMinutes,
                IsIfr = sectorDto.IsIfr,
                IsNight = sectorDto.IsNight,
                DayTakeoffs = sectorDto.DayTakeoffs,
                NightTakeoffs = sectorDto.NightTakeoffs,
                DayLandings = sectorDto.DayLandings,
                NightLandings = sectorDto.NightLandings,
            });
        }

        entity.CrewAssignments.Clear();
        foreach (var crewDto in dto.Crew)
        {
            entity.CrewAssignments.Add(new FlightLogCrew
            {
                CrewMemberId = crewDto.CrewMemberId,
                RoleType = crewDto.RoleType,
            });
        }
    }
}
