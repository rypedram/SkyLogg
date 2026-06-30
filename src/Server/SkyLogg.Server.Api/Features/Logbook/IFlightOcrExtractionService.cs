namespace SkyLogg.Server.Api.Features.Logbook;

public interface IFlightOcrExtractionService
{
    Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken);
}

public sealed partial class FlightOcrExtractionService : IFlightOcrExtractionService
{
    public async Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken)
    {
        if (contentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) is true ||
            HasTextExtension(fileName))
        {
            using var reader = new StreamReader(source);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        if (contentType?.Contains("pdf", StringComparison.OrdinalIgnoreCase) is true ||
            fileName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) is true)
        {
            return await ExtractPdfTextAsync(source, cancellationToken);
        }

        throw new BadRequestException("Unsupported file type for OCR import. Upload a text, CSV, or PDF file, or paste OCR text directly.");
    }

    private static bool HasTextExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName);
        return extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ExtractPdfTextAsync(Stream source, CancellationToken cancellationToken)
    {
        await using var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        // Text-based PDF extraction is handled in a later iteration; paste OCR output for scanned pages.
        using var reader = new StreamReader(memory);
        var content = await reader.ReadToEndAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(content) ? string.Empty : content;
    }
}
