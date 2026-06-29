using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLogExportService
{
    [AutoInject] private AppDbContext dbContext = default!;

    static FlightLogExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportCsvAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var logs = await GetLogsForExportAsync(userId, fromDate, toDate, cancellationToken);

        using var writer = new StringWriter();
        await writer.WriteLineAsync("Date,Aircraft,Route,Block,Flight,PIC,SIC,Dual,IFR,Night,Landings,Remarks");

        foreach (var log in logs)
        {
            var route = string.Join(" | ", log.Sectors.Select(s =>
                $"{s.DepartureAirport?.ICAO}-{s.ArrivalAirport?.ICAO}"));

            await writer.WriteLineAsync(string.Join(",",
                log.FlightDate.ToString("yyyy-MM-dd"),
                EscapeCsv(log.Aircraft?.Registration),
                EscapeCsv(route),
                FlightTimeFormatting.FormatMinutes(log.TotalBlockMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalFlightMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalPicMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalSicMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalDualMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalIfrMinutes),
                FlightTimeFormatting.FormatMinutes(log.TotalNightMinutes),
                log.TotalLandings.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(log.Remarks)));
        }

        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public async Task<byte[]> ExportPdfAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var logs = await GetLogsForExportAsync(userId, fromDate, toDate, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($"SkyLogg Flight Log ({fromDate:yyyy-MM-dd} — {toDate:yyyy-MM-dd})").FontSize(16).SemiBold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(60);
                        columns.RelativeColumn();
                        columns.ConstantColumn(45);
                        columns.ConstantColumn(45);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Date");
                        header.Cell().Text("A/C");
                        header.Cell().Text("Route");
                        header.Cell().Text("Block");
                        header.Cell().Text("Flight");
                    });

                    foreach (var log in logs)
                    {
                        var route = string.Join(" | ", log.Sectors.Select(s =>
                            $"{s.DepartureAirport?.ICAO}-{s.ArrivalAirport?.ICAO}"));

                        table.Cell().Text(log.FlightDate.ToString("yyyy-MM-dd"));
                        table.Cell().Text(log.Aircraft?.Registration ?? "");
                        table.Cell().Text(route);
                        table.Cell().Text(FlightTimeFormatting.FormatMinutes(log.TotalBlockMinutes));
                        table.Cell().Text(FlightTimeFormatting.FormatMinutes(log.TotalFlightMinutes));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task<List<FlightLog>> GetLogsForExportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        return await dbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Aircraft)
            .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
            .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
            .Where(f => f.UserId == userId && !f.Deleted && f.FlightDate >= fromDate && f.FlightDate <= toDate)
            .OrderBy(f => f.FlightDate)
            .ToListAsync(cancellationToken);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
