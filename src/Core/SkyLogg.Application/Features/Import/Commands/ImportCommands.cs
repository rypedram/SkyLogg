using MediatR;
using SkyLogg.Application.Features.Import.Dtos;
using SkyLogg.Domain.Enums;

namespace SkyLogg.Application.Features.Import.Commands;

public sealed record StartImportSessionCommand(ImportSourceType SourceType, string FileName) : IRequest<Guid>;

public sealed record ProcessImportCommand(Guid SessionId, Stream Source, string? ContentType) : IRequest<ImportPreviewDto>;

public sealed record ConfirmImportCommand(Guid SessionId, IReadOnlyList<ImportFlightCandidateDto> ApprovedFlights) : IRequest<ImportConfirmResultDto>;
