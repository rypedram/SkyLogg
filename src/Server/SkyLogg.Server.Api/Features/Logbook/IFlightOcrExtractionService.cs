namespace SkyLogg.Server.Api.Features.Logbook;

public interface IFlightOcrExtractionService
{
    Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken);
}

public sealed class PlaceholderFlightOcrExtractionService : IFlightOcrExtractionService
{
    public async Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken)
    {
        if (contentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) is true)
        {
            using var reader = new StreamReader(source);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        return "";
    }
}
