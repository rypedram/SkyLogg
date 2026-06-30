using FluentValidation;
using MediatR;
using SkyLogg.Application.Features.Aircraft.Dtos;

namespace SkyLogg.Application.Features.Aircraft.Queries;

public sealed record GetAircraftByIdQuery(Guid Id) : IRequest<AircraftReadDto?>;

public sealed record GetAircraftListQuery : IRequest<IReadOnlyList<AircraftReadDto>>;
