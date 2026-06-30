namespace SkyLogg.Application.Features.Map.Services;

/// <summary>
/// Prepares map visualization data. No rendering logic.
/// </summary>
public interface IFlightMapDataService
{
    Task<Dtos.FlightMapDataDto> GetMapDataAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? aircraftId,
        Guid? airportId,
        CancellationToken cancellationToken = default);
}
