namespace SkyLogg.Server.Api.Features.Logbook.Import.OurAirports;

public sealed partial class OurAirportsCatalogWarmupService : IHostedService
{
    [AutoInject] private OurAirportsCatalog catalog = default!;
    [AutoInject] private ILogger<OurAirportsCatalogWarmupService> logger = default!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await catalog.WarmUpAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "OurAirports catalog warm-up failed. Lookup will retry on first import.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
