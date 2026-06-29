namespace SkyLogg.Server.Api.Features.Logbook;

public interface IFlightMapProvider
{
    string Key { get; }

    string DisplayName { get; }

    bool SupportsAnimatedPlayback { get; }

    bool SupportsHeatmap { get; }
}

public sealed class SvgFlightMapProvider : IFlightMapProvider
{
    public string Key => "svg";

    public string DisplayName => "Built-in SVG Map";

    public bool SupportsAnimatedPlayback => false;

    public bool SupportsHeatmap => true;
}
