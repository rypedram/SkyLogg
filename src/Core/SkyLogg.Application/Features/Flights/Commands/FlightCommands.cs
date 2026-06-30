using MediatR;
using SkyLogg.Application.Features.Flights.Dtos;

namespace SkyLogg.Application.Features.Flights.Commands;

public sealed record CreateFlightLogCommand(FlightLogWriteDto Flight) : IRequest<Guid>;

public sealed record UpdateFlightLogCommand(Guid Id, long Version, FlightLogWriteDto Flight) : IRequest;

public sealed record DeleteFlightLogCommand(Guid Id, long Version) : IRequest;
