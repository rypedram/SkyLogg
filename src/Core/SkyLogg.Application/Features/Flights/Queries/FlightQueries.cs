using MediatR;
using SkyLogg.Application.Common.Models;
using SkyLogg.Application.Features.Flights.Dtos;

namespace SkyLogg.Application.Features.Flights.Queries;

public sealed record GetFlightLogByIdQuery(Guid Id) : IRequest<FlightLogReadDto?>;

public sealed record GetFlightLogsPagedQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<FlightLogReadDto>>;
