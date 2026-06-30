using MediatR;
using SkyLogg.Application.Features.Airports.Dtos;

namespace SkyLogg.Application.Features.Airports.Queries;

public sealed record SearchAirportsQuery(string Query, int Take = 20) : IRequest<IReadOnlyList<AirportReadDto>>;

public sealed record GetAirportByIdQuery(Guid Id) : IRequest<AirportReadDto?>;
