using System.Text.Json;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

internal static class FormStateHelper
{
    public static string Capture<T>(T value) => JsonSerializer.Serialize(value);

    public static bool HasChanges<T>(T current, string? snapshot)
        => snapshot is null || Capture(current) != snapshot;
}
