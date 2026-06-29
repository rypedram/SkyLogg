using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightImportPage
{
    [AutoInject] private IFlightImportController flightImportController = default!;

    private bool isPreviewing;
    private string? rawText;
    private FlightImportPreviewDto? preview;

    private async Task PreviewImport()
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return;

        isPreviewing = true;
        try
        {
            preview = await flightImportController.Preview(new FlightImportRequestDto
            {
                SourceType = FlightImportSourceType.RawText,
                RawText = rawText,
                FileName = "manual-ocr.txt",
            }, CurrentCancellationToken);
        }
        finally
        {
            isPreviewing = false;
        }
    }
}
