using MediatR;
using SkyLogg.Application.Features.Import.Dtos;

namespace SkyLogg.Application.Features.Import.Queries;

public sealed record GetImportSessionQuery(Guid SessionId) : IRequest<ImportSessionDto?>;
