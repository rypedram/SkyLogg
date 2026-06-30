using SkyLogg.Server.Api.Features.Logbook.Import;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class FlightImportController : AppControllerBase, IFlightImportController
{
    [AutoInject] private IFlightImportPipelineService importPipeline = default!;

    [HttpPost]
    public async Task<FlightImportPreviewDto> Preview(FlightImportRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError)]);

        return await importPipeline.PreviewAsync(User.GetUserId(), request, cancellationToken);
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<FlightImportPreviewDto> PreviewFromFile(
        IFormFile file,
        [FromForm] FlightImportSourceType sourceType,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException(Localizer[nameof(AppStrings.RequiredAttribute_ValidationError)]);

        await using var stream = file.OpenReadStream();
        return await importPipeline.PreviewFromFileAsync(
            User.GetUserId(),
            stream,
            file.FileName,
            file.ContentType,
            sourceType,
            cancellationToken);
    }

    [HttpPost]
    public async Task<FlightImportConfirmResultDto> Confirm(FlightImportConfirmDto request, CancellationToken cancellationToken)
    {
        return await importPipeline.ConfirmAsync(User.GetUserId(), request, cancellationToken);
    }
}
