using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightSummaryPage
{
    [AutoInject] private IFlightLogController flightLogController = default!;

    private bool isLoading;
    private DateTimeOffset? fromDate = DateTimeOffset.UtcNow.AddMonths(-1);
    private DateTimeOffset? toDate = DateTimeOffset.UtcNow;
    private FlightSummaryDto? summary;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadSummary();
    }

    private async Task LoadSummary()
    {
        isLoading = true;
        try
        {
            var from = DateOnly.FromDateTime((fromDate ?? DateTimeOffset.UtcNow).Date);
            var to = DateOnly.FromDateTime((toDate ?? DateTimeOffset.UtcNow).Date);
            summary = await flightLogController.GetSummary(from, to, CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task ExportCsv()
    {
        var from = DateOnly.FromDateTime((fromDate ?? DateTimeOffset.UtcNow).Date);
        var to = DateOnly.FromDateTime((toDate ?? DateTimeOffset.UtcNow).Date);
        var bytes = await flightLogController.ExportCsv(from, to, CurrentCancellationToken);
        await DownloadFile(bytes, $"flight-log-{from:yyyyMMdd}-{to:yyyyMMdd}.csv", "text/csv");
    }

    private async Task ExportPdf()
    {
        var from = DateOnly.FromDateTime((fromDate ?? DateTimeOffset.UtcNow).Date);
        var to = DateOnly.FromDateTime((toDate ?? DateTimeOffset.UtcNow).Date);
        var bytes = await flightLogController.ExportPdf(from, to, CurrentCancellationToken);
        await DownloadFile(bytes, $"flight-log-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf", "application/pdf");
    }

    private async Task DownloadFile(byte[] bytes, string fileName, string mimeType)
    {
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("App.downloadFile", fileName, mimeType, base64);
    }
}
