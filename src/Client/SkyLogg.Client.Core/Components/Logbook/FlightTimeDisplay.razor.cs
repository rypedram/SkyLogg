using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Logbook;

public partial class FlightTimeDisplay
{
    [Parameter] public int Minutes { get; set; }

    private string FormattedTime => FlightTimeFormatting.FormatMinutes(Minutes);
}
