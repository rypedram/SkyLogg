using SkyLogg.Application.Features.Import.Services;

namespace SkyLogg.Infrastructure.External.Ocr;

public sealed class MockOcrProvider : IOcrProvider
{
    public async Task<string> ExtractTextAsync(Stream source, string? fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        if (contentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) is true)
        {
            using var reader = new StreamReader(source);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        return """
            05 JAN 2026  D-AIAB  A320  OIII-EDDF
            OFF 08:10  T/O 08:25  LDG 10:15  ON 10:30
            Training flight
            """;
    }
}
