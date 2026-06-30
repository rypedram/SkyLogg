using MediatR;
using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Application.Features.Flights.Commands;
using SkyLogg.Application.Features.Flights.Dtos;
using SkyLogg.Application.Features.Flights.Queries;
using SkyLogg.Domain.Entities;
using SkyLogg.Domain.Services;

namespace SkyLogg.Application.Features.Flights.Services;

public static class FlightLogMapper
{
    public static FlightLog ToEntity(FlightLogWriteDto dto, Guid userId)
    {
        var flight = new FlightLog
        {
            UserId = userId,
            AircraftId = dto.AircraftId,
            FlightDate = dto.FlightDate,
            Remarks = dto.Remarks
        };

        MapSectors(flight, dto.Sectors);
        MapCrew(flight, dto.Crew);
        Recalculate(flight);
        return flight;
    }

    public static void Apply(FlightLog flight, FlightLogWriteDto dto)
    {
        flight.AircraftId = dto.AircraftId;
        flight.FlightDate = dto.FlightDate;
        flight.Remarks = dto.Remarks;
        flight.Sectors.Clear();
        flight.CrewAssignments.Clear();
        MapSectors(flight, dto.Sectors);
        MapCrew(flight, dto.Crew);
        Recalculate(flight);
    }

    public static FlightLogReadDto ToReadDto(FlightLog flight)
    {
        return new FlightLogReadDto
        {
            Id = flight.Id,
            Version = flight.Version,
            AircraftId = flight.AircraftId,
            FlightDate = flight.FlightDate,
            Remarks = flight.Remarks,
            TotalBlockMinutes = flight.TotalBlockMinutes,
            TotalFlightMinutes = flight.TotalFlightMinutes,
            TotalLandings = flight.TotalLandings,
            Sectors = flight.Sectors.OrderBy(s => s.SectorOrder).Select(s => new FlightSectorDto
            {
                Id = s.Id,
                SectorOrder = s.SectorOrder,
                DepartureAirportId = s.DepartureAirportId,
                ArrivalAirportId = s.ArrivalAirportId,
                BlockOff = s.BlockOff,
                BlockOn = s.BlockOn,
                Takeoff = s.Takeoff,
                Landing = s.Landing,
                PicTimeMinutes = s.PicTimeMinutes,
                SicTimeMinutes = s.SicTimeMinutes,
                DualTimeMinutes = s.DualTimeMinutes,
                NightTimeMinutes = s.NightTimeMinutes,
                IfrTimeMinutes = s.IfrTimeMinutes,
                IsIfr = s.IsIfr,
                IsNight = s.IsNight,
                DayLandings = s.DayLandings,
                NightLandings = s.NightLandings
            }).ToList(),
            Crew = flight.CrewAssignments.Select(c => new FlightCrewAssignmentDto
            {
                CrewMemberId = c.CrewMemberId,
                RoleType = c.RoleType
            }).ToList()
        };
    }

    private static void MapSectors(FlightLog flight, IEnumerable<FlightSectorDto> sectors)
    {
        var order = 1;
        foreach (var sectorDto in sectors)
        {
            var duration = FlightCalculationService.CalculateSectorDuration(
                sectorDto.BlockOff, sectorDto.BlockOn, sectorDto.Takeoff, sectorDto.Landing);

            var sector = new FlightSector
            {
                FlightLogId = flight.Id,
                SectorOrder = order++,
                DepartureAirportId = sectorDto.DepartureAirportId,
                ArrivalAirportId = sectorDto.ArrivalAirportId,
                BlockOff = sectorDto.BlockOff,
                BlockOn = sectorDto.BlockOn,
                Takeoff = sectorDto.Takeoff,
                Landing = sectorDto.Landing,
                BlockTimeMinutes = duration.BlockMinutes,
                FlightTimeMinutes = duration.FlightMinutes,
                PicTimeMinutes = sectorDto.PicTimeMinutes,
                SicTimeMinutes = sectorDto.SicTimeMinutes,
                DualTimeMinutes = sectorDto.DualTimeMinutes,
                NightTimeMinutes = sectorDto.NightTimeMinutes,
                IfrTimeMinutes = sectorDto.IfrTimeMinutes,
                IsIfr = sectorDto.IsIfr,
                IsNight = sectorDto.IsNight,
                DayLandings = sectorDto.DayLandings,
                NightLandings = sectorDto.NightLandings
            };

            FlightValidationService.ValidateSector(sector);
            flight.Sectors.Add(sector);
        }
    }

    private static void MapCrew(FlightLog flight, IEnumerable<FlightCrewAssignmentDto> crew)
    {
        foreach (var assignment in crew)
        {
            flight.CrewAssignments.Add(new FlightCrew
            {
                FlightLogId = flight.Id,
                CrewMemberId = assignment.CrewMemberId,
                RoleType = assignment.RoleType
            });
        }
    }

    private static void Recalculate(FlightLog flight)
    {
        FlightCalculationService.ApplyTotals(flight);
    }
}

public sealed class CreateFlightLogCommandHandler : IRequestHandler<CreateFlightLogCommand, Guid>
{
    private readonly IFlightLogRepository flightLogs;
    private readonly IAircraftRepository aircraft;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUserAccessor currentUser;

    public CreateFlightLogCommandHandler(
        IFlightLogRepository flightLogs,
        IAircraftRepository aircraft,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        this.flightLogs = flightLogs;
        this.aircraft = aircraft;
        this.unitOfWork = unitOfWork;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateFlightLogCommand request, CancellationToken cancellationToken)
    {
        var aircraftEntity = await aircraft.GetByIdAsync(request.Flight.AircraftId, cancellationToken)
            ?? throw new InvalidOperationException("Aircraft not found.");

        var flight = FlightLogMapper.ToEntity(request.Flight, currentUser.UserId);
        FlightValidationService.ValidateFlightLog(flight, aircraftEntity);

        flightLogs.Add(flight);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return flight.Id;
    }
}

public sealed class UpdateFlightLogCommandHandler : IRequestHandler<UpdateFlightLogCommand>
{
    private readonly IFlightLogRepository flightLogs;
    private readonly IAircraftRepository aircraft;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUserAccessor currentUser;

    public UpdateFlightLogCommandHandler(
        IFlightLogRepository flightLogs,
        IAircraftRepository aircraft,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        this.flightLogs = flightLogs;
        this.aircraft = aircraft;
        this.unitOfWork = unitOfWork;
        this.currentUser = currentUser;
    }

    public async Task Handle(UpdateFlightLogCommand request, CancellationToken cancellationToken)
    {
        var flight = await flightLogs.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Flight log not found.");

        if (flight.Version != request.Version)
            throw new InvalidOperationException("Concurrency conflict.");

        var aircraftEntity = await aircraft.GetByIdAsync(request.Flight.AircraftId, cancellationToken)
            ?? throw new InvalidOperationException("Aircraft not found.");

        FlightLogMapper.Apply(flight, request.Flight);
        FlightValidationService.ValidateFlightLog(flight, aircraftEntity);
        flight.UpdatedAt = DateTimeOffset.UtcNow;
        flight.Version++;

        flightLogs.Update(flight);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteFlightLogCommandHandler : IRequestHandler<DeleteFlightLogCommand>
{
    private readonly IFlightLogRepository flightLogs;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUserAccessor currentUser;

    public DeleteFlightLogCommandHandler(
        IFlightLogRepository flightLogs,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        this.flightLogs = flightLogs;
        this.unitOfWork = unitOfWork;
        this.currentUser = currentUser;
    }

    public async Task Handle(DeleteFlightLogCommand request, CancellationToken cancellationToken)
    {
        var flight = await flightLogs.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Flight log not found.");

        if (flight.Version != request.Version)
            throw new InvalidOperationException("Concurrency conflict.");

        flight.Deleted = true;
        flight.UpdatedAt = DateTimeOffset.UtcNow;
        flight.Version++;

        flightLogs.Update(flight);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetFlightLogByIdQueryHandler : IRequestHandler<GetFlightLogByIdQuery, FlightLogReadDto?>
{
    private readonly IFlightLogRepository flightLogs;
    private readonly ICurrentUserAccessor currentUser;

    public GetFlightLogByIdQueryHandler(IFlightLogRepository flightLogs, ICurrentUserAccessor currentUser)
    {
        this.flightLogs = flightLogs;
        this.currentUser = currentUser;
    }

    public async Task<FlightLogReadDto?> Handle(GetFlightLogByIdQuery request, CancellationToken cancellationToken)
    {
        var flight = await flightLogs.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken);
        return flight is null ? null : FlightLogMapper.ToReadDto(flight);
    }
}

public sealed class GetFlightLogsPagedQueryHandler : IRequestHandler<GetFlightLogsPagedQuery, Common.Models.PagedResult<FlightLogReadDto>>
{
    private readonly IFlightLogRepository flightLogs;
    private readonly ICurrentUserAccessor currentUser;

    public GetFlightLogsPagedQueryHandler(IFlightLogRepository flightLogs, ICurrentUserAccessor currentUser)
    {
        this.flightLogs = flightLogs;
        this.currentUser = currentUser;
    }

    public async Task<Common.Models.PagedResult<FlightLogReadDto>> Handle(GetFlightLogsPagedQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await flightLogs.GetPagedAsync(
            currentUser.UserId,
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);

        return new Common.Models.PagedResult<FlightLogReadDto>
        {
            Items = items.Select(FlightLogMapper.ToReadDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
