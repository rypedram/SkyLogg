using MediatR;
using SkyLogg.Application.Features.Map.Dtos;

namespace SkyLogg.Application.Features.Map.Queries;

public sealed record GetFlightMapDataQuery(
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? AircraftId = null,
    Guid? AirportId = null) : IRequest<FlightMapDataDto>;
