namespace SkyLogg.Server.Api.Features.Logbook.Import;

public static class FlightImportTextExtractor
{
    public static List<FlightImportRow> ExtractRows(string rawText)
    {
        var csvRows = FlightImportParser.Parse(rawText);
        if (csvRows.Count > 0)
            return csvRows;

        return OcrFlightTextParser.Parse(rawText);
    }
}
