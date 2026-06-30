namespace SkyLogg.Server.Api.Features.Logbook.Import;

public sealed class ResolvedAircraftTypeInfo
{
    public required string Manufacturer { get; init; }

    public required string Model { get; init; }

    public required string TypeCode { get; init; }

    public string? Category { get; init; }

    public string? EngineType { get; init; }

    public int? EngineCount { get; init; }
}
