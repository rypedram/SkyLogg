using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightSummaryPage
{
    [AutoInject] private IFlightLogController flightLogController = default!;

    private bool isLoading;
    private string? loadError;
    private DateTimeOffset? fromDate = DateTimeOffset.UtcNow.AddYears(-10);
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
        loadError = null;

        try
        {
            var (from, to) = GetDateRange();
            summary = await flightLogController.GetSummary(from, to, CurrentCancellationToken);
        }
        catch (KnownException e)
        {
            summary = null;
            loadError = e.Message;
            SnackBarService.Error(e.Message);
        }
        catch (Exception e)
        {
            summary = null;
            loadError = e.Message;
            SnackBarService.Error(Localizer[nameof(AppStrings.FlightSummaryLoadFailed)]);
        }
        finally
        {
            isLoading = false;
        }
    }

    private (DateOnly From, DateOnly To) GetDateRange()
    {
        var from = DateOnly.FromDateTime((fromDate ?? DateTimeOffset.UtcNow.AddYears(-10)).Date);
        var to = DateOnly.FromDateTime((toDate ?? DateTimeOffset.UtcNow).Date);

        if (from > to)
            (from, to) = (to, from);

        return (from, to);
    }

    private async Task ExportCsv()
    {
        var (from, to) = GetDateRange();
        var bytes = await flightLogController.ExportCsv(from, to, CurrentCancellationToken);
        await DownloadFile(bytes, $"flight-log-{from:yyyyMMdd}-{to:yyyyMMdd}.csv", "text/csv");
    }

    private async Task ExportPdf()
    {
        var (from, to) = GetDateRange();
        var bytes = await flightLogController.ExportPdf(from, to, CurrentCancellationToken);
        await DownloadFile(bytes, $"flight-log-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf", "application/pdf");
    }

    private async Task DownloadFile(byte[] bytes, string fileName, string mimeType)
    {
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("App.downloadFile", fileName, mimeType, base64);
    }
}
