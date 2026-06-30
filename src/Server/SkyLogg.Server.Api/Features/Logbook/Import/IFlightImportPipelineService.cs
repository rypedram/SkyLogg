using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook.Import;

public interface IFlightImportPipelineService
{
    Task<FlightImportPreviewDto> PreviewAsync(Guid userId, FlightImportRequestDto request, CancellationToken cancellationToken = default);

    Task<FlightImportPreviewDto> PreviewFromFileAsync(
        Guid userId,
        Stream fileStream,
        string fileName,
        string? contentType,
        FlightImportSourceType sourceType,
        CancellationToken cancellationToken = default);

    Task<FlightImportConfirmResultDto> ConfirmAsync(Guid userId, FlightImportConfirmDto request, CancellationToken cancellationToken = default);
}
