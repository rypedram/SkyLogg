namespace SkyLogg.Shared.Features.Logbook;

[Mapper(UseDeepCloning = true)]
public static partial class LogbookMapper
{
    public static partial void Patch(this FlightLogDto source, FlightLogDto destination);

    public static partial void Patch(this FlightSectorDto source, FlightSectorDto destination);
}
