using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using SkyLogg.Shared.Features.Logbook;
using SkyLogg.Shared.Infrastructure.Dtos;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightImportPage
{
    [AutoInject] private IFlightImportController flightImportController = default!;
    [AutoInject] private HttpClient httpClient = default!;
    [AutoInject] private AuthManager authManager = default!;

    private bool isPreviewing;
    private bool isConfirming;
    private bool isUploading;
    private string? rawText;
    private FlightImportPreviewDto? preview;

    private IEnumerable<FlightImportCandidateDto> SelectedCandidates =>
        preview?.Candidates.Where(c => c.IsSelected && c.IsValid && c.IsDuplicate is false && c.ProposedFlight is not null) ?? [];

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

    private async Task OnImportFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null)
            return;

        isUploading = true;
        isPreviewing = true;

        try
        {
            await using var stream = file.OpenReadStream(20 * 1024 * 1024, CurrentCancellationToken);
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", file.Name);
            content.Add(new StringContent(ResolveSourceType(file).ToString("D")), "sourceType");

            using var request = new HttpRequestMessage(HttpMethod.Post, GetUploadPreviewUrl());
            var accessToken = await authManager.GetFreshAccessToken(requestedBy: nameof(FlightImportPage));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = content;

            using var response = await httpClient.SendAsync(request, CurrentCancellationToken);
            response.EnsureSuccessStatusCode();

            preview = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.FlightImportPreviewDto, CurrentCancellationToken);
            rawText = preview?.RawText;
        }
        catch (KnownException ex)
        {
            SnackBarService.Error(ex.Message);
        }
        catch (HttpRequestException)
        {
            SnackBarService.Error(Localizer[nameof(AppStrings.FileUploadFailed)]);
        }
        finally
        {
            isUploading = false;
            isPreviewing = false;
        }
    }

    private string GetUploadPreviewUrl()
    {
        var url = new Uri(AbsoluteServerAddress, "/api/v1/FlightImport/PreviewFromFile").ToString();
        if (CultureInfoManager.InvariantGlobalization is false)
            url += $"?culture={CultureInfo.CurrentUICulture.Name}";

        return url;
    }

    private static FlightImportSourceType ResolveSourceType(IBrowserFile file)
    {
        var contentType = file.ContentType ?? string.Empty;
        var extension = Path.GetExtension(file.Name);

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return FlightImportSourceType.Image;

        if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return FlightImportSourceType.Pdf;

        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return FlightImportSourceType.Csv;

        return FlightImportSourceType.RawText;
    }

    private async Task ConfirmImport()
    {
        if (preview is null)
            return;

        var selected = SelectedCandidates.ToList();
        if (selected.Count == 0)
            return;

        isConfirming = true;
        try
        {
            var result = await flightImportController.Confirm(new FlightImportConfirmDto
            {
                ImportHistoryId = preview.ImportHistoryId,
                Flights = selected.Select(c => c.ProposedFlight!).ToList(),
                LineNumbers = selected.Select(c => c.LineNumber).ToList(),
            }, CurrentCancellationToken);

            ApplySkippedDuplicates(result);

            if (result.SavedCount > 0 && result.SkippedDuplicateCount > 0)
            {
                SnackBarService.Success(string.Format(
                    Localizer[nameof(AppStrings.ImportFlightsPartialSaved)],
                    result.SavedCount,
                    result.SkippedDuplicateCount));
            }
            else if (result.SavedCount > 0)
            {
                SnackBarService.Success(Localizer[nameof(AppStrings.ImportFlightsSaved)]);
                NavigationManager.NavigateTo(PageUrls.FlightLogs);
            }
            else if (result.SkippedDuplicateCount > 0)
            {
                SnackBarService.Error(Localizer[nameof(AppStrings.ImportAllDuplicatesSkipped)]);
            }
        }
        finally
        {
            isConfirming = false;
        }
    }

    private void ApplySkippedDuplicates(FlightImportConfirmResultDto result)
    {
        if (preview is null || result.SkippedDuplicateLineNumbers.Count == 0)
            return;

        var overlapMessage = Localizer[nameof(AppStrings.FlightLogOverlapConflict)];
        foreach (var lineNumber in result.SkippedDuplicateLineNumbers)
        {
            var candidate = preview.Candidates.FirstOrDefault(c => c.LineNumber == lineNumber);
            if (candidate is null)
                continue;

            candidate.IsDuplicate = true;
            candidate.IsValid = false;
            candidate.IsSelected = false;

            if (candidate.ValidationWarnings.Contains(overlapMessage) is false)
                candidate.ValidationWarnings.Add(overlapMessage);
        }

        preview.ValidCount = preview.Candidates.Count(c => c.IsValid);
        preview.InvalidCount = preview.Candidates.Count(c => !c.IsValid);
        preview.DuplicateCount = preview.Candidates.Count(c => c.IsDuplicate);
    }

    private static string? GetRowClass(FlightImportCandidateDto candidate)
        => candidate.IsDuplicate ? "import-row-duplicate" : null;

    private static BitColor ConfidenceColor(double confidence) => confidence switch
    {
        >= 0.9 => BitColor.Success,
        >= 0.75 => BitColor.Warning,
        _ => BitColor.Error
    };

    private static BitColor ValidityColor(bool isValid) => isValid ? BitColor.Success : BitColor.Error;
}
